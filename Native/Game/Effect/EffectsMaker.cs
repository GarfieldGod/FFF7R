using System.ComponentModel;

public class EffectsMaker {
    public EffectsMaker(List<List<int>> PosStatusMap, InputerType inputerType = InputerType.PLAYER) {
        posStatus_ = PosStatusMap;
        inputerType_ = inputerType;
    }
    private Dictionary<Int2D, Tuple<ChessProperty, int>> effectTasks_ = new Dictionary<Int2D, Tuple<ChessProperty, int>>();
    private InputerType inputerType_;
    private List<List<int>> posStatus_;
    private List<List<int>> effectStatusBase_ = Utils.DeepCopy2DList(Utils.EmptyStandard2DList);
    private List<List<int>> effectStatusEnhanced_ = Utils.DeepCopy2DList(Utils.EmptyStandard2DList);

    public bool AddEffectsTask(Int2D effectPos, ChessProperty property) {
        if (effectTasks_.ContainsKey(effectPos)) {
            return false;
        }
        if (property.CardEffectConfig.condition < EffectCondition.ON_POSITION) {
            CardEffect.ParseCardEffect(effectPos, property, effectStatusEnhanced_, UsePosStatus());
        } else if (property.CardEffectConfig.condition >= EffectCondition.CoverInput) {
            //special
        } else {
            //many times
            int initTimes = 0;
            if (property.CardEffectConfig.condition == EffectCondition.ON_POSITION) {
                initTimes = 1;
            }
            effectTasks_.Add(effectPos, new Tuple<ChessProperty, int>(property, initTimes));
        }
        return true;
    }
    public bool RemoveEffectsTask(Int2D effectPos) {
        if (effectTasks_.ContainsKey(effectPos)) {
            effectTasks_.Remove(effectPos);
            return true;
        }
        return false;
    }
    public int GetEffectStatus(Int2D effectPos) {
        return GetEffectStatusBase(effectPos) + GetEffectStatusEnhanced(effectPos);
    }
    public int GetEffectStatusBase(Int2D effectPos) {
        return effectStatusBase_[effectPos.x][effectPos.y];
    }
    public int GetEffectStatusEnhanced(Int2D effectPos) {
        return effectStatusEnhanced_[effectPos.x][effectPos.y];
    }
    public List<List<int>> ComposeCardEffectsMap() {
        List<List<int>> effectEnhancedFinal = Utils.Compose2DList(effectStatusEnhanced_, CalculateEnhancedEffectsList());
        return Utils.Compose2DList(effectEnhancedFinal, effectStatusBase_);
    }
    public List<List<int>> CalculateEnhancedEffectsList() {
        List<List<int>> result = Utils.DeepCopy2DList(Utils.EmptyStandard2DList);
        foreach(var effectTask in effectTasks_) {
            for(int i = 0; i <= effectTask.Value.Item2;i++) {
                CardEffect.ParseCardEffect(effectTask.Key, effectTask.Value.Item1, result, UsePosStatus());
            }
        }
        return result;
    }
    public void TriggerEffectByCondition(EffectCondition condition){
        List<Int2D> pos = new List<Int2D>();
        foreach(var effectTask in effectTasks_) {
            if (effectTask.Value.Item1.CardEffectConfig.condition == condition) {
                pos.Add(effectTask.Key);
            }
        }
        foreach(var vaildPos in pos) {
            effectTasks_[vaildPos] = new Tuple<ChessProperty, int>(effectTasks_[vaildPos].Item1, effectTasks_[vaildPos].Item2 + 1);
        }
    }
    private List<List<int>> UsePosStatus(){
        List<List<int>> result = Utils.DeepCopy2DList(posStatus_);
        if (inputerType_ == InputerType.RIVAL) {
            Rival.GetChessPosStatusInRivalView(result);
        }
        return result;
    }
    private List<List<int>> UseEffectStatusBase(){
        List<List<int>> result = Utils.DeepCopy2DList(effectStatusBase_);
        if (inputerType_ == InputerType.RIVAL) {
            Rival.GetChessLevelStatusInRivalView(result);
        }
        return result;
    }
    private List<List<int>> UseEffectStatusEnhanced(){
        List<List<int>> result = Utils.DeepCopy2DList(effectStatusEnhanced_);
        if (inputerType_ == InputerType.RIVAL) {
            Rival.GetChessLevelStatusInRivalView(result);
        }
        return result;
    }
}

static class PosEffect {
    public static List<List<int>> DoPosEffect(Int2D chessGridPos, List<List<int>> posEffects, List<List<int>> chessGridPosEffectStatus) {
        List<List<int>> chessGridStatusTemp = Utils.DeepCopy2DList(chessGridPosEffectStatus);
        chessGridStatusTemp[chessGridPos.x][chessGridPos.y] = (int)ChessPosStatus.OCCUPIED_FRIEND;
        Int2D chessPadSize = new Int2D(chessGridStatusTemp.Count, chessGridStatusTemp[0].Count);
        var effectTask = EffectsParser.ParseEffectsInPosition(chessPadSize, chessGridPos,  EffectsParser.ParseEffectsInRelative(posEffects, true));
        return ExecutePosEffect(effectTask, chessGridStatusTemp);
    }
    private static List<List<int>> ExecutePosEffect(List<Tuple<Int2D, int>> effectTask, List<List<int>> chessGridStatus) {
        foreach (Tuple<Int2D, int> Task in effectTask) {
            int pastLevel = chessGridStatus[Task.Item1.x][Task.Item1.y];
            if ((ChessPosStatus)pastLevel >= ChessPosStatus.OCCUPIED_FRIEND) {
                continue;
            }
            int newLevel = pastLevel % 10;
            if (pastLevel <= (int)ChessPosStatus.EMPTY) {
                newLevel += Task.Item2;
            }
            if (newLevel > (int)ChessPosStatus.LEVEL_THREE_FRIEND) {
                newLevel = (int)ChessPosStatus.LEVEL_THREE_FRIEND;
            }
            chessGridStatus[Task.Item1.x][Task.Item1.y] = newLevel;
        }
        return chessGridStatus;
    }
}

static class CardEffect {
    public static List<Tuple<Int2D, int>> ParseCardEffect(Input input, ChessPad chessPad) {
        ChessProperty property = input.chess.GetChessProperty();
        var intiEffectTasks = EffectsParser.ParseEffectsInRelative(property.CardEffects.Item3, false);
        Int2D padSize = chessPad.GetSize();
        Int2D gridPos = input.pos;
        var vaildTasksInPosition = EffectsParser.ParseEffectsInPosition(padSize, gridPos, intiEffectTasks);
        return vaildTasksInPosition;
    }
    public static List<Tuple<Int2D, int>> ParseCardEffectInScope(Input input, ChessPad chessPad) {
        ChessProperty property = input.chess.GetChessProperty();
        var vaildTasksInPosition = ParseCardEffect(input, chessPad);
        var gridLevelMap = chessPad.GetChessGridStatus();
        var vaildTasksInScope = GetEffectInEffectScope(property.CardEffects.Item1, vaildTasksInPosition, gridLevelMap);
        return vaildTasksInScope;
    }
    public static List<List<int>> DoCardEffect(Input parms, ChessPad chessPad) {
        ChessProperty property = parms.chess.GetChessProperty();
        return DoCardEffect(parms.pos, property, chessPad);
    }
    public static List<List<int>> DoCardEffect(Int2D pos, ChessProperty property, ChessPad chessPad) {
        List<List<int>> GridMap = chessPad.GetChessGridStatus();
        List<List<int>> LevelMap = Utils.DeepCopy2DList(chessPad.GetCardLevelResult());
        Tuple<EffectScope, EffectCondition, List<List<int>>> cardEffects = property.CardEffects;
        if (cardEffects == null || cardEffects.Item3 == null || cardEffects.Item3.Count == 0) {
            return LevelMap;
        }
        Int2D chessPadSize = new Int2D(GridMap.Count, GridMap[0].Count);
        var effectTask = EffectsParser.ParseEffectsInPosition(chessPadSize, pos,  EffectsParser.ParseEffectsInRelative(cardEffects.Item3, false));
        var taskToRun = GetEffectInEffectScope(cardEffects.Item1, effectTask, GridMap);
        ExecutCardEffect(taskToRun, LevelMap);
        return LevelMap;
    }
    public static List<List<int>> ParseCardEffect(Int2D chessGridPos, ChessProperty property, List<List<int>> effectsMap, List<List<int>> posMap) {
        List<List<int>> chessGridStatusTemp = Utils.DeepCopy2DList(effectsMap);
        Tuple<EffectScope, EffectCondition, List<List<int>>> cardEffects = property.CardEffects;
        if (cardEffects == null || cardEffects.Item3 == null || cardEffects.Item3.Count == 0) {
            return chessGridStatusTemp;
        }
        Int2D chessPadSize = new Int2D(chessGridStatusTemp.Count, chessGridStatusTemp[0].Count);
        var effectTask = EffectsParser.ParseEffectsInPosition(chessPadSize, chessGridPos,  EffectsParser.ParseEffectsInRelative(cardEffects.Item3, false));
        var taskToRun = GetEffectInEffectScope(cardEffects.Item1, effectTask, posMap);
        ExecutCardEffect(taskToRun, chessGridStatusTemp);
        return chessGridStatusTemp;
    }
    // public static void ChooseCardEffectsType(Int2D chessPos, ChessProperty chessProperty, ChessPadInfo chessPadInfo) {
    //     switch (chessProperty.CardEffects.Item2) {
    //         case CardEffectsType.ON_PLAYED:
    //             break;
    //         case CardEffectsType.ON_POSITION:

    //             break;
    //         case CardEffectsType.ON_SELF_DEAD:
    //         case CardEffectsType.ON_ENEMY_DEAD:
    //         case CardEffectsType.ON_ENHANCED:
    //             chessPadInfo.delayEffectsList.Add(chessPos, chessProperty);
    //             break;
    //         default: break;
    //     }
    // }
    // public static void RemoveCardDelayEffects(Int2D chessPos, Dictionary<Int2D, List<Tuple<Int2D, int>>> delayEffectsList) {
    //     if (delayEffectsList.ContainsKey(chessPos)) {
    //         delayEffectsList.Remove(chessPos);
    //     }
    // }
    // private static List<List<int>> ComposeOneAndLastingCardEffects(List<List<int>> chessGridCardEffectsStatus, Dictionary<Int2D, List<Tuple<Int2D, int>>> tasksLasting) {
    //     List<List<int>> chessGridCardEffectsStatusTemp = chessGridCardEffectsStatus.Select(innerList => new List<int>(innerList)).ToList();
    //     foreach(var taskPair in tasksLasting) {
    //         ExecutCardEffect(taskPair.Value, chessGridCardEffectsStatusTemp);
    //     }
    //     return chessGridCardEffectsStatusTemp;
    // }
    private static List<Tuple<Int2D, int>> GetEffectInEffectScope(EffectScope cardEffectsScope, List<Tuple<Int2D, int>> effectTask, List<List<int>> chessGridPosEffectStatus) {
        var tasksToRun = new List<Tuple<Int2D, int>>{};
        foreach(var task in effectTask) {
            switch (cardEffectsScope) {
                case EffectScope.DOTOALL:
                    tasksToRun.Add(task);
                    break;
                case EffectScope.FRIEND_ONLY:
                    if (chessGridPosEffectStatus[task.Item1.x][task.Item1.y] == (int)ChessPosStatus.OCCUPIED_FRIEND) {
                        tasksToRun.Add(task);
                    }
                    break;
                case EffectScope.ENEMY_ONLY:
                    if (chessGridPosEffectStatus[task.Item1.x][task.Item1.y] == (int)ChessPosStatus.OCCUPIED_ENEMY) {
                        tasksToRun.Add(task);
                    }
                    break;
                case EffectScope.FRIEND_INCREASE_ENEMY_REDUCE_ONCE:
                    if ((chessGridPosEffectStatus[task.Item1.x][task.Item1.y] == (int)ChessPosStatus.OCCUPIED_ENEMY  && task.Item2 < 0) ||
                        (chessGridPosEffectStatus[task.Item1.x][task.Item1.y] == (int)ChessPosStatus.OCCUPIED_FRIEND && task.Item2 > 0)) {
                        tasksToRun.Add(task);
                    }
                    if (chessGridPosEffectStatus[task.Item1.x][task.Item1.y] == (int)ChessPosStatus.OCCUPIED_FRIEND) {
                        tasksToRun.Add(task);
                    } else if (chessGridPosEffectStatus[task.Item1.x][task.Item1.y] == (int)ChessPosStatus.OCCUPIED_ENEMY) {
                        Tuple<Int2D, int> newTask = new Tuple<Int2D, int>(task.Item1, -task.Item2);
                        tasksToRun.Add(newTask);
                    }
                    break;
                default:break;
            }
        }
        return tasksToRun;
    }
    private static void ExecutCardEffect(List<Tuple<Int2D, int>> effectTask, List<List<int>> chessGridStatus) {
        foreach (Tuple<Int2D, int> Task in effectTask) {
            chessGridStatus[Task.Item1.x][Task.Item1.y] += Task.Item2;
        }
    }
    // public static List<List<int>> DoDelayedCardEffect(Int2D chessGridPos, ChessProperty property, ChessPadInfo chessPadInfo) {
    //     List<List<int>> chessGridPosEffectStatus = chessPadInfo.chessPadStatus[0];
    //     List<List<int>> chessGridStatusTemp = GlobalScope.DeepCopy2DList(chessPadInfo.chessPadStatus[2]);
    //     Tuple<CardEffectsScope, CardEffectsType, List<List<int>>> cardEffects = property.CardEffects;
    //     if (cardEffects == null || cardEffects.Item3 == null || cardEffects.Item3.Count == 0) {
    //         return chessGridStatusTemp;
    //     }
    //     Int2D chessPadSize = new Int2D(chessGridStatusTemp.Count, chessGridStatusTemp[0].Count);
    //     var effectTask = EffectsParser.ParseEffectsInPosition(chessPadSize, chessGridPos,  EffectsParser.ParseEffectsInRelative(cardEffects.Item3, false));
    //     // var taskToRun = GetVaildCardEffectTasks(cardEffects.Item1, effectTask, chessGridPosEffectStatus);
    //     ExecutCardEffect(effectTask, chessGridStatusTemp);
    //     return chessGridStatusTemp;
    // }
}