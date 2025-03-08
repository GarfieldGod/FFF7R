using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessDispenser : MonoBehaviour
{
    public GameObject chessSelector;
    public GameObject chessPrefab;
    // Start is called before the first frame update
    void Awake()
    {
        GlobalScope.LoadChessProperties();
    }
    void Start()
    {
        StartAGame();
    }
    void StartAGame() {
        chessPool = new List<string>{
            "Card001", "Card001", "Card001", "Card001", "Card001",
            "Card002", "Card002", "Card002", "Card002", "Card002",
            "Card003", "Card003", "Card003", "Card003", "Card003",
        };
        Debug.Log("Instantiate chessPool success.");
        for( int i = 0 ; i < 5 ; i++ ) {
            ChessSelector.PushBackChess(InstantiateChess(DispenseChess()));
        }
    }
    void Update() {}

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
            ChessSelector.PushBackChess(InstantiateChess(DispenseChess()));
        }
        foreach(Chess chess in popChessList) {
            chessPool.Add(chess.name);
        }
    }

    public Chess InstantiateChess(string chessName)
    {
        GameObject instantiateBody = Instantiate(chessPrefab, chessSelector.transform);
        GlobalScope.ChessProperty instantiateData = GlobalScope.GetChessProperty(chessName);
        if (instantiateData != null) {
            return new Chess(instantiateData, instantiateBody);
        }
        return null;
    }

    public int GetChessNumInChessSelector() {
        int result = 0;
        foreach(Transform child in chessSelector.transform) {
            result++;
        }
        return result;
    }
}
