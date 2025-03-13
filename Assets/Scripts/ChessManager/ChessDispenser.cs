using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessDispenser : MonoBehaviour
{
    public GameObject chessSelectorPos;
    public GameObject chessPrefab;
    public static List<string> chessPool = new List<string>{};
    public static string DispenseChess() {
        if (chessPool.Count == 0) {
            return null;
        }
        System.Random random = new System.Random();
        int randomIndex = random.Next(chessPool.Count);
        string chess = chessPool[randomIndex];
        chessPool.RemoveAt(randomIndex);
        return chess;
    }

    public void ReDispense(List<int> index) {
        List<Chess> popChessList = new List<Chess>{};
        foreach(int i in index) {
            if(i < 0 || i >= ChessSelector.chessList.Count) {
                Debug.Log("ReDispense: Invaild chessIndex.");
                continue;
            }
            popChessList.Add(ChessSelector.chessList[i]);
        }
        foreach(Chess chess in popChessList) {
            ChessSelector.chessList.Remove(chess);
        }
        foreach(Chess chess in popChessList) {
            ChessSelector.PushBackChess(InstantiateChess(DispenseChess()).GetComponent<Chess>());
        }
        foreach(Chess chess in popChessList) {
            chessPool.Add(chess.cardCode);
        }
    }

    public GameObject InstantiateChess(string chessName)
    {
        if (chessName == null) {
            return null;
        }
        GameObject instantiateBody = Instantiate(chessPrefab, chessSelectorPos.transform);
        ChessProperty instantiateData = GlobalScope.GetChessProperty(chessName);
        if (instantiateData != null) {
            instantiateBody.GetComponent<Chess>().InstantiateChessProperty(instantiateData);
            return instantiateBody;
        }
        return null;
    }

    public int GetChessNumInChessSelector() {
        int result = 0;
        foreach(Transform child in chessSelectorPos.transform) {
            result++;
        }
        return result;
    }
}
