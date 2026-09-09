static class CardEffect {
    // 效果在棋盘中的实际位置
    public static List<Tuple<Int2D, int>> ParseCardEffect(Input input, ChessPad chessPad) {
        var cardEffectsMap = input.chess.Property.CardEffects.Item3;
        if (input.playerType == PlayerType.RIVAL)
        {
            cardEffectsMap = Utils.Reverse(cardEffectsMap);
        }
        var intiEffectTasks = EffectsParser.ParseEffectsInRelative(cardEffectsMap, false);
        var validTasksInPosition = EffectsParser.ParseEffectsInPosition(chessPad.Size, input.pos, intiEffectTasks);
        return validTasksInPosition;
    }

    // 在棋盘中实际位置可以生效的效果
    public static List<Tuple<Int2D, int>> ParseCardEffectInScope(Input input, ChessPad chessPad) {
        EffectScope scope = input.chess.Property.CardEffects.Item1;
        var gridLevelMap = chessPad.StatusMap;
        var validTasksInPosition = ParseCardEffect(input, chessPad);
        var validTasksInScope = GetEffectInScope(scope, validTasksInPosition, gridLevelMap);
        return validTasksInScope;
    }

    private static List<Tuple<Int2D, int>> GetEffectInScope(EffectScope scope, List<Tuple<Int2D, int>> effectTask, List<List<int>> posStatusMap)
    {
        var tasks = new List<Tuple<Int2D, int>>{};

        foreach(var task in effectTask) {
            PosStatus posStatus = (PosStatus)posStatusMap[task.Item1.x][task.Item1.y]; 
            switch (scope) {
                case EffectScope.DOTOALL:
                    if (posStatus == PosStatus.OCCUPIED_PLAYER || posStatus == PosStatus.OCCUPIED_RIVAL) {
                        tasks.Add(task);
                    }
                    break;
                case EffectScope.FRIEND_ONLY:
                    if (posStatus == PosStatus.OCCUPIED_PLAYER) {
                        tasks.Add(task);
                    }
                    break;
                case EffectScope.ENEMY_ONLY:
                    if (posStatus == PosStatus.OCCUPIED_RIVAL) {
                        tasks.Add(task);
                    }
                    break;
                case EffectScope.FRIEND_INCREASE_ENEMY_REDUCE_ONCE:
                    if (posStatus == PosStatus.OCCUPIED_PLAYER) {
                        tasks.Add(task);
                    } else if (posStatus == PosStatus.OCCUPIED_RIVAL) {
                        Tuple<Int2D, int> newTask = new Tuple<Int2D, int>(task.Item1, -task.Item2);
                        tasks.Add(newTask);
                    }
                    break;
                default:break;
            }
        }
        return tasks;
    }

    // public static List<List<int>> ParseCardEffect(Int2D chessGridPos, ChessProperty property, List<List<int>> effectsMap, List<List<int>> posMap) {
    //     List<List<int>> gridStatusTemp = Utils.DeepCopy2DList(effectsMap);
    //     Tuple<EffectScope, EffectCondition, List<List<int>>> cardEffects = property.CardEffects;
    //     if (cardEffects == null || cardEffects.Item3 == null || cardEffects.Item3.Count == 0) {
    //         return gridStatusTemp;
    //     }
    //     Int2D chessPadSize = new Int2D(gridStatusTemp.Count, gridStatusTemp[0].Count);
    //     var effectTask = EffectsParser.ParseEffectsInPosition(chessPadSize, chessGridPos,  EffectsParser.ParseEffectsInRelative(cardEffects.Item3, false));
    //     var taskToRun = GetEffectInScope(cardEffects.Item1, effectTask, posMap);
    //     ExecutCardEffect(taskToRun, gridStatusTemp);
    //     return gridStatusTemp;
    // }

    // private static void ExecutCardEffect(List<Tuple<Int2D, int>> effectTask, List<List<int>> chessGridStatus) {
    //     foreach (Tuple<Int2D, int> Task in effectTask) {
    //         chessGridStatus[Task.Item1.x][Task.Item1.y] += Task.Item2;
    //     }
    // }
}