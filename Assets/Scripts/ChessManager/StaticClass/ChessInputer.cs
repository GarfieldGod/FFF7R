using System.Collections.Generic;
using System;
using System.Linq;
using Unity.VisualScripting;


#if UNITY_ENGINE
using UnityEngine;
public struct ChessInputParmObj {
    public GameObject chessGrid;
    public GameObject chessObj;
    public List<List<List<int>>> chessGridStatus;
    public Dictionary<Int2D, List<Tuple<Int2D, int>>> tasksLasting;
    public bool rivalInput;
    public ChessInputParmObj(GameObject chessGrid, GameObject chessObj, List<List<List<int>>> chessGridStatus, Dictionary<Int2D, List<Tuple<Int2D, int>>> tasksLasting, bool rivalInput = false) {
        this.chessGrid = chessGrid;
        this.chessObj = chessObj;
        this.chessGridStatus = chessGridStatus;
        this.tasksLasting = tasksLasting;
        this.rivalInput = rivalInput;
    }
}
public struct ChessInputParm {
    public Int2D chessGridPos;
    public ChessProperty property;
    public List<List<List<int>>> chessGridStatus;
    public Dictionary<Int2D, List<Tuple<Int2D, int>>> tasksLasting;
    public bool rivalInput;
    public ChessInputParm(Int2D chessGridPos, ChessProperty property, List<List<List<int>>> chessGridStatus, Dictionary<Int2D, List<Tuple<Int2D, int>>> tasksLasting, bool rivalInput = false) {
        this.chessGridPos = chessGridPos;
        this.property = property;
        this.chessGridStatus = chessGridStatus;
        this.tasksLasting = tasksLasting;
        this.rivalInput = rivalInput;
    }
}
public class ChessInputer : MonoBehaviour {
    public static void GetChessInput(ChessInputParmObj parms) {
        GetChessInput(parms.chessGrid, parms.chessObj, parms.chessGridStatus, parms.tasksLasting, parms.rivalInput);
    }
    public static void GetChessInput(ChessInputParm parms) {
        GetChessInput(parms.chessGridPos, parms.property, parms.chessGridStatus, parms.tasksLasting, parms.rivalInput);
    }
    public static void GetChessInput(GameObject chessGrid, GameObject chessObj, List<List<List<int>>> chessGridStatus, Dictionary<Int2D, List<Tuple<Int2D, int>>> tasksLasting, bool rivalInput) {
        Int2D chessGridPos = chessGrid.GetComponent<ChessGrid>().chessGridPos_;
        ChessProperty property = GlobalScope.GetChessProperty(chessObj.name);
        GetChessInput(chessGridPos, property, chessGridStatus, tasksLasting, rivalInput);
        ChessSelector.RemoveChess(chessObj);
    }
    public static void GetChessInput(Int2D chessGridPos, ChessProperty property, List<List<List<int>>> chessGridStatus, Dictionary<Int2D, List<Tuple<Int2D, int>>> tasksLasting, bool rivalInput) {
        chessGridStatus[0] = PosEffect.DoPosEffect(chessGridPos, property.PosEffects, chessGridStatus[0]);
        chessGridStatus[1] = CardEffect.DoCardEffect(chessGridPos, property, chessGridStatus, tasksLasting);
        GetCardModelOn(chessGridPos, property, rivalInput);
    }
    private static void GetCardModelOn(Int2D chessGridPos, ChessProperty property, bool rivalInput) { // TODO somebug in rival view
        Int2D finalPos = chessGridPos;
        if(rivalInput) {
            finalPos = Rival.GetChessGridPosInRivalView(finalPos);
        }
        GameObject chessGridObj = GlobalScope.GetChessGridObjectByChessGridPos(finalPos);
        GameObject instantiateModel = ChessDispenser.InstantiateChessModel(GlobalScope.chessModelPrefab_static_, chessGridObj, property, true);
        instantiateModel.transform.localPosition = new Vector3(0.5f, -0.8f, 0);
        instantiateModel.transform.localRotation = Quaternion.Euler(-90, 0, 0);
        instantiateModel.transform.localScale = new Vector3(90, 80, 150);
        // TextMesh level = instantiateModel.transform.Find("level").GetComponent<TextMesh>();
        // chessGridObj.GetComponent<ChessGrid>().levelText_ = level;
    }
    public static List<List<List<int>>> GetPreviewChessGridStatus(ChessInputParmObj parms) {
        Int2D chessGridPos = parms.chessGrid.GetComponent<ChessGrid>().chessGridPos_;
        ChessProperty property = GlobalScope.GetChessProperty(parms.chessObj.name);
        return GetPreviewChessGridStatus(chessGridPos, property, parms.chessGridStatus, parms.tasksLasting);
    }
    public static List<List<List<int>>> GetPreviewChessGridStatus(ChessInputParm parms) {
        return GetPreviewChessGridStatus(parms.chessGridPos, parms.property, parms.chessGridStatus, parms.tasksLasting);
    }
    public static List<List<List<int>>> GetPreviewChessGridStatus(Int2D chessGridPos, ChessProperty property, List<List<List<int>>> chessGridStatus, Dictionary<Int2D, List<Tuple<Int2D, int>>> tasksLasting) {
        List<List<List<int>>> chessGridStatusTemp = GlobalScope.DeepCopy3DList(chessGridStatus);
        Dictionary<Int2D, List<Tuple<Int2D, int>>> tasksLastingTemp = new Dictionary<Int2D, List<Tuple<Int2D, int>>>();
        chessGridStatusTemp[0] = PosEffect.DoPosEffect(chessGridPos, property.PosEffects, chessGridStatusTemp[0]);
        chessGridStatusTemp[1] = CardEffect.DoCardEffect(chessGridPos, property, chessGridStatusTemp, tasksLastingTemp);
        return chessGridStatusTemp;
    }
}
#endif

public class CardEffect {
    public static List<List<int>> DoCardEffect(ChessInputParm parms) {
        return DoCardEffect(parms.chessGridPos, parms.property, parms.chessGridStatus, parms.tasksLasting);
    }
    public static List<List<int>> DoCardEffect(Int2D chessGridPos, ChessProperty property, List<List<List<int>>> chessGridStatus, Dictionary<Int2D, List<Tuple<Int2D, int>>> tasksLasting) {
        List<List<int>> chessGridPosEffectStatus = chessGridStatus[0];
        List<List<int>> chessGridCardEffectsStatus = chessGridStatus[1];
        List<List<int>> chessGridStatusTemp = GlobalScope.DeepCopy2DList(chessGridCardEffectsStatus);
        chessGridStatusTemp[chessGridPos.x][chessGridPos.y] = property.Level;
        Tuple<CardEffectsScope, CardEffectsType, List<List<int>>> cardEffects = property.CardEffects;
        if(cardEffects == null || cardEffects.Item3 == null || cardEffects.Item3.Count == 0) {
            return chessGridStatusTemp;
        }
        Int2D chessPadSize = new Int2D(chessGridStatusTemp.Count, chessGridStatusTemp[0].Count);
        var effectTask = EffectsParser.ParseEffectsInPosition(chessPadSize, chessGridPos,  EffectsParser.ParseEffectsInRelative(cardEffects.Item3, false));
        if(cardEffects.Item2 <= CardEffectsType.ON_PLAYED) {
            var taskToRun = GetVaildCardEffectTasks(cardEffects.Item1, effectTask, chessGridPosEffectStatus);
            ExecuteCardEffect(taskToRun, chessGridStatusTemp);
        } else {
            // tasksLasting.Add(chessGridPos, effectTask);
        }
        return ComposeOneAndLastingCardEffects(chessGridStatusTemp, tasksLasting);
        }
    public static void RemoveCardLastingEffect(Int2D chessPos, Dictionary<Int2D, List<Tuple<Int2D, int>>> tasksLasting) {
        if (tasksLasting.ContainsKey(chessPos)) {
            tasksLasting.Remove(chessPos);
        }
    }
    private static List<List<int>> ComposeOneAndLastingCardEffects(List<List<int>> chessGridCardEffectsStatus, Dictionary<Int2D, List<Tuple<Int2D, int>>> tasksLasting) {
        List<List<int>> chessGridCardEffectsStatusTemp = chessGridCardEffectsStatus.Select(innerList => new List<int>(innerList)).ToList();
        foreach(var taskPair in tasksLasting) {
            ExecuteCardEffect(taskPair.Value, chessGridCardEffectsStatusTemp);
        }
        return chessGridCardEffectsStatusTemp;
    }
    private static List<Tuple<Int2D, int>> GetVaildCardEffectTasks(CardEffectsScope cardEffectsScope, List<Tuple<Int2D, int>> effectTask, List<List<int>> chessGridPosEffectStatus) {
        var tasksToRun = new List<Tuple<Int2D, int>>{};
        foreach(var task in effectTask) {
            switch (cardEffectsScope) {
                case CardEffectsScope.DOTOALL:
                    tasksToRun.Add(task);
                    break;
                case CardEffectsScope.FRIEND_ONLY:
                    if (chessGridPosEffectStatus[task.Item1.x][task.Item1.y] == (int)ChessPosStatus.OCCUPIED_FRIEND) {
                        tasksToRun.Add(task);
                    }
                    break;
                case CardEffectsScope.ENEMY_ONLY:
                    if (chessGridPosEffectStatus[task.Item1.x][task.Item1.y] == (int)ChessPosStatus.OCCUPIED_ENEMY) {
                        tasksToRun.Add(task);
                    }
                    break;
                case CardEffectsScope.FRIEND_INCREASE_ENEMY_REDUCE_ONCE:
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
    private static void ExecuteCardEffect(List<Tuple<Int2D, int>> effectTask, List<List<int>> chessGridStatus) {
        foreach (Tuple<Int2D, int> Task in effectTask) {
            int pastLevel = chessGridStatus[Task.Item1.x][Task.Item1.y];
            int newLevel = pastLevel + Task.Item2;
            chessGridStatus[Task.Item1.x][Task.Item1.y] = newLevel;
        }
    }
}

public class PosEffect {
    public static List<List<int>> DoPosEffect(Int2D chessGridPos, List<List<int>> posEffects, List<List<int>> chessGridPosEffectStatus) {
        List<List<int>> chessGridStatusTemp = chessGridPosEffectStatus.Select(innerList => new List<int>(innerList)).ToList();
        chessGridStatusTemp[chessGridPos.x][chessGridPos.y] = (int)ChessPosStatus.OCCUPIED_FRIEND;
        Int2D chessPadSize = new Int2D(chessGridStatusTemp.Count, chessGridStatusTemp[0].Count);
        var effectTask = EffectsParser.ParseEffectsInPosition(chessPadSize, chessGridPos,  EffectsParser.ParseEffectsInRelative(posEffects, true));
        ExecutePosEffect(effectTask, chessGridStatusTemp);
        return chessGridStatusTemp;
    }
    private static void ExecutePosEffect(List<Tuple<Int2D, int>> effectTask, List<List<int>> chessGridStatus) {
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
    }
}

class EffectsParser {
    public static List<Tuple<Int2D, int>> ParseEffectsInRelative(List<List<int>> effects, bool ignoreSelf)
    {
        var result = new List<Tuple<Int2D, int>>();
        var midPos = new Int2D((effects.Count - 1) / 2, (effects[0].Count - 1) / 2);
        for (int i = 0; i < effects.Count; i++) {
            for (int j = 0; j < effects[i].Count; j++) {
                if (effects[i][j] != 0) {
                    int posX = i - midPos.x;
                    int posY = j - midPos.y;
                    var posWithValue = new Tuple<Int2D, int>(new Int2D(posX, posY), effects[i][j]);
                    if(ignoreSelf) {
                        if (posX != 0 || posY != 0) {
                            result.Add(posWithValue);
                        }
                    } else {
                        result.Add(posWithValue);
                    }
                    // Log.test($"ParseEffectsInRelative: y: {posY} x: {posX} value: {effects[i][j]}");
                }
            }
        }
        return result;
    }

    public static List<Tuple<Int2D, int>> ParseEffectsInPosition(Int2D chessPadSize, Int2D posPos, List<Tuple<Int2D, int>> parsedEffects)
    {
        List<Tuple<Int2D, int>> result = new List<Tuple<Int2D, int>>();
        if (posPos.x < 0 || posPos.y < 0) {
            return result;
        }
        foreach (var effect in parsedEffects) {
            int posX = posPos.x + effect.Item1.x;
            int posY = posPos.y + effect.Item1.y;
            if (posX <  chessPadSize.x && posX >= 0 && posY <  chessPadSize.y && posY >= 0) {
                Log.test($"ParseEffectsInPosition: vertical: {posY} horizontal: {posX} value: {effect.Item2}");
                var affectedPos = new Int2D(posX, posY);
                var next = new Tuple<Int2D, int>(affectedPos, effect.Item2);
                // var next = new Tuple<string, int>(GlobalScope.chessPositionNameList[posY, posX], effect.Item2); //use chessGrid name in the past
                result.Add(next);
            }
        }
        return result;
    }
}
