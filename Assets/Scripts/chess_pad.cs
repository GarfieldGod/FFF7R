using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class chess_pad : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void GetChessInput(GameObject chessPos, string chessName) {
        GetCardModelOn();
        GlobalScope.ChessProperty property = GlobalScope.GetChessProperty(chessName);
        DoPosEffect(chessPos.name, property.PosEffects);
        DoCardEffect(chessPos.name, property.CardEffects);
        DoSpecialEffect(chessPos.name, property.SpecialEffects);
        chessPos.GetComponent<chess_position>().posStatus = GlobalScope.ChessPosStatus.OCCUPIED_FRIEND;
    }
    void GetCardModelOn() {

    }
    void DoPosEffect(string chessPosName, List<List<int>> effects) {
        var effectTask = ParseEffectsInPosition(chessPosName,  ParseEffects(effects));
        ExecutePosEffect(effectTask);
    }
    void DoCardEffect(string chessPosName, List<List<int>> effects) {
        // TODO
    }
    void DoSpecialEffect(string chessPosName, HashSet<string> effects) {
        // TODO
    }
    void ExecutePosEffect(List<Tuple<string, int>> effectTask) {
        foreach (Tuple<string, int> Task in effectTask) {
            Debug.Log("TaskName : " + Task.Item1);
            foreach (Transform child in transform)
            {
                if (child.gameObject.name == Task.Item1) {
                    chess_position chessPos = child.gameObject.GetComponent<chess_position>();
                    Debug.Log(child.gameObject.name + " Plus: " + Task.Item2);
                    chessPos.level += Task.Item2;
                    if (chessPos.posStatus >= GlobalScope.ChessPosStatus.OCCUPIED_FRIEND) {
                        continue;
                    }
                    int chessLevel = (int)chessPos.posStatus % 10 + Task.Item2;
                    if (chessLevel > (int)GlobalScope.ChessPosStatus.LEVEL_THREE_FRIEND) {
                        chessLevel = (int)GlobalScope.ChessPosStatus.LEVEL_THREE_FRIEND;
                    }
                    chessPos.posStatus = (GlobalScope.ChessPosStatus)chessLevel;
                    chessPos.level = chessLevel;
                }
            }
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
                        // Debug.Log($"y: {posY} x: {posX} value: {effects[i][j]}");
                    }
                }
            }
        }
        return result;
    }

    static List<Tuple<string, int>> ParseEffectsInPosition(string posName, List<Tuple<Tuple<int, int>, int>> parsedEffects)
    {
        List<Tuple<string, int>> result = new List<Tuple<string, int>>();
        var posPos = new Tuple<int, int>(-1, -1);
        for (int i = 0; i < GlobalScope.chessPositionNameList.GetLength(0); i++) {
            for (int j = 0; j < GlobalScope.chessPositionNameList.GetLength(1); j++) {
                if ( GlobalScope.chessPositionNameList[i, j] == posName) {
                    posPos = new Tuple<int, int>(i, j);
                }
            }
        }
        if (posPos.Item1 < 0 || posPos.Item2 < 0) {
            return result;
        }
        // Debug.Log($"posPos: {posPos.Item1} {posPos.Item2}");
        foreach (var effect in parsedEffects) {
            int posY = posPos.Item1 + effect.Item1.Item1;
            int posX = posPos.Item2 + effect.Item1.Item2;
            if (posY <  GlobalScope.chessPositionNameList.GetLength(0) && posY >= 0 && posX <  GlobalScope.chessPositionNameList.GetLength(1) && posX >= 0) {
                var affectedPos = new Tuple<int, int>(posY, posX);
                var next = new Tuple<string, int>(GlobalScope.chessPositionNameList[posY, posX], effect.Item2);
                result.Add(next);
                // Debug.Log($"affectedPos: { GlobalScope.chessPositionNameList[posY, posX]} \tpos: ({affectedPos.Item1}, {affectedPos.Item2}) effect: {effect.Item2}");
            }
        }
        return result;
    }
}
