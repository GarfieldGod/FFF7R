using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
#if UNITY_ENGINE
using UnityEngine;
public class ChessInputer : MonoBehaviour {
    public GameObject chessPad;
    public void GetChessInput(GameObject chessGrid, GameObject chessObj) {
        //chessObj
        GlobalScope.ChessProperty property = GlobalScope.GetChessProperty(chessObj.name);
        ChessSelector.RemoveChess(chessObj);
        //chessGrid
        Int2D chessGridPos = chessGrid.GetComponent<ChessGrid>().chessGridPos;
        GetCardModelOn(chessGrid, property);
        GlobalScope.chessGridStatus = DoPosEffect(chessGridPos, property.PosEffects, GlobalScope.chessGridStatus);
        DoCardEffect(chessGridPos, property.CardEffects);
        DoSpecialEffect(chessGridPos, property.SpecialEffects);
        GameManager.CommitChessStatusToChessPad();
    }
    void GetCardModelOn(GameObject chessGridPos, GlobalScope.ChessProperty property) {
        // TODO
    }
    // List<List<int>> GetChessInputWithChessGridStatus(List<List<int>> ChessGridStatus, Int2D chessGridPos, string chessName) {
    //     List<List<int>> result = new List<List<int>>(ChessGridStatus);
    //     DoPosEffect(chessGridPos, property.PosEffects, GlobalScope.chessGridStatus);
    //     return result;
    // }
#else
using System.Numerics;
public class ChessInputer {
#endif
    public static List<List<int>> DoPosEffect(Int2D chessGridPos, List<List<int>> posEffects, List<List<int>> chessGridPosEffectStatus) {
        List<List<int>> chessGridStatusTemp = chessGridPosEffectStatus.Select(innerList => new List<int>(innerList)).ToList();
        chessGridStatusTemp[chessGridPos.x][chessGridPos.y] = (int)ChessPosStatus.OCCUPIED_FRIEND;
        Int2D chessPadSize = new Int2D(chessGridStatusTemp.Count, chessGridStatusTemp[0].Count);
        var effectTask = ParseEffectsInPosition(chessPadSize, chessGridPos,  ParseEffectsInRelative(posEffects));
        ExecutePosEffect(effectTask, chessGridStatusTemp);
        return chessGridStatusTemp;
    }
    private static void ExecutePosEffect(List<Tuple<Int2D, int>> effectTask, List<List<int>> chessGridStatus) {
        foreach (Tuple<Int2D, int> Task in effectTask) {
            int pastLevel = chessGridStatus[Task.Item1.x][Task.Item1.y];
            if ((ChessPosStatus)pastLevel >= ChessPosStatus.OCCUPIED_FRIEND) {
                continue;
            }
            int newLevel = (pastLevel % 10) + Task.Item2;
            if (newLevel > (int)ChessPosStatus.LEVEL_THREE_FRIEND) {
                newLevel = (int)ChessPosStatus.LEVEL_THREE_FRIEND;
            }
            chessGridStatus[Task.Item1.x][Task.Item1.y] = newLevel;
        }
    }
    public static List<List<int>> DoCardEffect(Int2D chessGridPos, Tuple<CardEffectsType, List<List<int>>> cardEffects, List<List<int>> chessGridCardEffectsStatus, List<List<int>> chessGridPosEffectStatus) {
        List<List<int>> chessGridStatusTemp = chessGridCardEffectsStatus.Select(innerList => new List<int>(innerList)).ToList();
        Int2D chessPadSize = new Int2D(chessGridStatusTemp.Count, chessGridStatusTemp[0].Count);
        var effectTask = ParseEffectsInPosition(chessPadSize, chessGridPos,  ParseEffectsInRelative(cardEffects.Item2));
        if(cardEffects.Item1 <= CardEffectsType.FRIEND_INCREASE_ENEMY_REDUCE_ONCE) {
            var taskToRun = GetVaildCardEffectTasks(cardEffects.Item1, effectTask, chessGridPosEffectStatus);
            ExecuteCardEffect(taskToRun, chessGridStatusTemp);
        } else {
            tasksLasting_.Add(chessGridPos, effectTask);
        }
        return ComposeOneAndLastingCardEffects(chessGridStatusTemp);;
    }
    private static List<Tuple<Int2D, int>> GetVaildCardEffectTasks(CardEffectsType cardEffectsType, List<Tuple<Int2D, int>> effectTask, List<List<int>> chessGridPosEffectStatus) {
        var absoluteCardEffectsType = (CardEffectsType)((int)cardEffectsType % 10);
        var tasksToRun = new List<Tuple<Int2D, int>>{};
        foreach(var task in effectTask) {
            switch (absoluteCardEffectsType) {
                case CardEffectsType.DOTOALL_ONCE:
                    tasksToRun.Add(task);
                    break;
                case CardEffectsType.FRIEND_ONLY_ONCE:
                    if (chessGridPosEffectStatus[task.Item1.x][task.Item1.y] == (int)ChessPosStatus.OCCUPIED_FRIEND) {
                        tasksToRun.Add(task);
                    }
                    break;
                case CardEffectsType.ENEMY_ONLY_ONCE:
                    if (chessGridPosEffectStatus[task.Item1.x][task.Item1.y] == (int)ChessPosStatus.OCCUPIED_ENEMY) {
                        tasksToRun.Add(task);
                    }
                    break;
                case CardEffectsType.FRIEND_INCREASE_ENEMY_REDUCE_ONCE:
                    if ((chessGridPosEffectStatus[task.Item1.x][task.Item1.y] == (int)ChessPosStatus.OCCUPIED_ENEMY  && task.Item2 < 0) ||
                        (chessGridPosEffectStatus[task.Item1.x][task.Item1.y] == (int)ChessPosStatus.OCCUPIED_FRIEND && task.Item2 > 0)) {
                        tasksToRun.Add(task);
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
    private static Dictionary<Int2D, List<Tuple<Int2D, int>>> tasksLasting_ = new Dictionary<Int2D, List<Tuple<Int2D, int>>>{};
    private static List<List<int>> ComposeOneAndLastingCardEffects(List<List<int>> chessGridCardEffectsStatus) {
        List<List<int>> chessGridCardEffectsStatusTemp = chessGridCardEffectsStatus.Select(innerList => new List<int>(innerList)).ToList();
        foreach(var taskPair in tasksLasting_) {
            ExecuteCardEffect(taskPair.Value, chessGridCardEffectsStatusTemp);
        }
        return chessGridCardEffectsStatusTemp;
        // for(int i = 0; i < chessGridCardEffectsStatus.Count; i++) {
        //     for(int j = 0; j < chessGridCardEffectsStatus[i].Count; j++) {
                
        //     }
        // }
    }
    void DoSpecialEffect(Int2D chessGridPos, HashSet<string> effects) {
        // TODO
    }
    private static List<Tuple<Int2D, int>> ParseEffectsInRelative(List<List<int>> effects)
    {
        var result = new List<Tuple<Int2D, int>>();
        var midPos = new Int2D((effects.Count - 1) / 2, (effects[0].Count - 1) / 2);
        for (int i = 0; i < effects.Count; i++) {
            for (int j = 0; j < effects[i].Count; j++) {
                if (effects[i][j] != 0) {
                    int posX = i - midPos.x;
                    int posY = j - midPos.y;
                    if (posX != 0 || posY != 0) {
                        var posWithValue = new Tuple<Int2D, int>(new Int2D(posX, posY), effects[i][j]);
                        result.Add(posWithValue);
                        Log.test($"ParseEffectsInRelative: y: {posY} x: {posX} value: {effects[i][j]}");
                    }
                }
            }
        }
        return result;
    }

    private static List<Tuple<Int2D, int>> ParseEffectsInPosition(Int2D chessPadSize, Int2D posPos, List<Tuple<Int2D, int>> parsedEffects)
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
