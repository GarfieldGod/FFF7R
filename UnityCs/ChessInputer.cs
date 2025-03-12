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
        Tuple<int, int> chessGridPos = chessGrid.GetComponent<ChessGrid>().chessGridPos;
        GetCardModelOn(chessGrid, property);
        GlobalScope.chessGridStatus = DoPosEffect(chessGridPos, property.PosEffects, GlobalScope.chessGridStatus);
        DoCardEffect(chessGridPos, property.CardEffects);
        DoSpecialEffect(chessGridPos, property.SpecialEffects);
        GameManager.CommitChessStatusToChessPad();
    }
    void GetCardModelOn(GameObject chessGridPos, GlobalScope.ChessProperty property) {
        // TODO
    }
    // List<List<int>> GetChessInputWithChessGridStatus(List<List<int>> ChessGridStatus, Tuple<int, int> chessGridPos, string chessName) {
    //     List<List<int>> result = new List<List<int>>(ChessGridStatus);
    //     DoPosEffect(chessGridPos, property.PosEffects, GlobalScope.chessGridStatus);
    //     return result;
    // }
#else
public class ChessInputer {
#endif
    public static List<List<int>> DoPosEffect(Tuple<int, int> chessGridPos, List<List<int>> effects, List<List<int>> chessGridStatus) {
        List<List<int>> chessGridStatusTemp = chessGridStatus.Select(innerList => new List<int>(innerList)).ToList();
        chessGridStatusTemp[chessGridPos.Item1][chessGridPos.Item2] = (int)GlobalScope.ChessPosStatus.OCCUPIED_FRIEND;
        var effectTask = ParseEffectsInPosition(chessGridPos,  ParseEffects(effects));
        ExecutePosEffect(effectTask, chessGridStatusTemp);
        return chessGridStatusTemp;
    }
    void DoCardEffect(Tuple<int, int> chessGridPos, List<List<int>> effects) {
        // TODO
    }
    void DoSpecialEffect(Tuple<int, int> chessGridPos, HashSet<string> effects) {
        // TODO
    }

    static void ExecutePosEffect(List<Tuple<Tuple<int, int>, int>> effectTask, List<List<int>> chessGridStatus) {
        foreach (Tuple<Tuple<int, int>, int> Task in effectTask) {
            int pastLevel = chessGridStatus[Task.Item1.Item1][Task.Item1.Item2];
            if ((GlobalScope.ChessPosStatus)pastLevel >= GlobalScope.ChessPosStatus.OCCUPIED_FRIEND) {
                continue;
            }
            int newLevel = (pastLevel % 10) + Task.Item2;
            if (newLevel > (int)GlobalScope.ChessPosStatus.LEVEL_THREE_FRIEND) {
                newLevel = (int)GlobalScope.ChessPosStatus.LEVEL_THREE_FRIEND;
            }
            chessGridStatus[Task.Item1.Item1][Task.Item1.Item2] = newLevel;
        }
    }

    static List<Tuple<Tuple<int, int>, int>> ParseEffects(List<List<int>> effects)
    {
        var result = new List<Tuple<Tuple<int, int>, int>>();
        var midPos = new Tuple<int, int>((effects.Count - 1) / 2, (effects[0].Count - 1) / 2);
        for (int i = 0; i < effects.Count; i++) {
            for (int j = 0; j < effects[i].Count; j++) {
                if (effects[i][j] != 0) {
                    int posY = i - midPos.Item1;
                    int posX = j - midPos.Item2;
                    if (posX != 0 || posY != 0) {
                        var posWithValue = new Tuple<Tuple<int, int>, int>(new Tuple<int, int>(posY, posX), effects[i][j]);
                        result.Add(posWithValue);
                        Log.test($"y: {posY} x: {posX} value: {effects[i][j]}");
                    }
                }
            }
        }
        return result;
    }

    static List<Tuple<Tuple<int, int>, int>> ParseEffectsInPosition(Tuple<int, int> posPos, List<Tuple<Tuple<int, int>, int>> parsedEffects)
    {
        List<Tuple<Tuple<int, int>, int>> result = new List<Tuple<Tuple<int, int>, int>>();
        if (posPos.Item1 < 0 || posPos.Item2 < 0) {
            return result;
        }
        foreach (var effect in parsedEffects) {
            int posY = posPos.Item1 + effect.Item1.Item1;
            int posX = posPos.Item2 + effect.Item1.Item2;
            if (posY <  GlobalScope.chessPositionNameList.GetLength(0) && posY >= 0 && posX <  GlobalScope.chessPositionNameList.GetLength(1) && posX >= 0) {
                Log.test($"vertical: {posY} horizontal: {posX} value: {effect.Item2}");
                var affectedPos = new Tuple<int, int>(posY, posX);
                var next = new Tuple<Tuple<int, int>, int>(affectedPos, effect.Item2);
                // var next = new Tuple<string, int>(GlobalScope.chessPositionNameList[posY, posX], effect.Item2); //use chessGrid name in the past
                result.Add(next);
            }
        }
        return result;
    }
}
