using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    ChessDispenser chessDispenser;
    static GameObject chessPad;
    public static List<List<int>> chessGridLevel;
    public static List<List<int>> chessGridStatus;
    void Start()
    {
        chessDispenser = GetComponent<ChessDispenser>();
        chessPad = GameObject.Find("chessPad");
        StartAGame();
    }
    void StartAGame() {
        InitChessPadStandard();
        ChessDispenser.chessPool = new List<string>{
            "Card001", "Card001", "Card001", "Card001", "Card001",
            "Card002", "Card002", "Card002", "Card002", "Card002",
            "Card003", "Card003", "Card003", "Card003", "Card003",
        };
        Debug.Log("Instantiate chessPool success.");
        for( int i = 0 ; i < 5 ; i++ ) {
            ChessSelector.PushBackChess(chessDispenser.InstantiateChess(ChessDispenser.DispenseChess()).GetComponent<Chess>());
        }
        InitChessPadStandard();
    }
    void InitChessPadStandard() {
        chessGridLevel = new List<List<int>>{
            new List<int> { 1, 0, 0, 0, 1 },
            new List<int> { 1, 0, 0, 0, 1 },
            new List<int> { 1, 0, 0, 0, 1 },
        };
        chessGridStatus = new List<List<int>>{
            new List<int> { 1, 10, 10, 10, 11 },
            new List<int> { 1, 10, 10, 10, 11 },
            new List<int> { 1, 10, 10, 10, 11 },
        };
        CommitChessPad();
    }

    static void CommitChessPad(){
        for(int i = 0; i < GlobalScope.chessPositionNameList.GetLength(0); i++) {
            for(int j = 0; j < GlobalScope.chessPositionNameList.GetLength(1); j++) {
                Transform chessGrid = chessPad.transform.Find(GlobalScope.chessPositionNameList[i, j]);
                if(Enum.IsDefined(typeof(GlobalScope.ChessPosStatus), chessGridStatus[i][j]) && chessGrid != null) {
                    chessGrid.GetComponent<ChessGrid>().posStatus = (GlobalScope.ChessPosStatus)chessGridStatus[i][j];
                }
            }
        }
    }
}
