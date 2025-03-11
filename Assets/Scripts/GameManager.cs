using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    public enum GameStatus {
        INIT,
        REDISPENSE_CHESS,
        GAMING,
        COMPUTE_RESULT,
        GAME_OVER
    }
    ChessDispenser chessDispenser;
    static GameObject chessPad;

    public GameStatus gameStatus = GameStatus.INIT;
    public int GameTurns = 0;
    public bool PlayerTurn = true;
    List<string> rivalChessListGlabol;
//----------------------------------------------------------------------------------------------------------------------------------InitGame
    void Start()
    {
        chessDispenser = GetComponent<ChessDispenser>();
        chessPad = GameObject.Find("chessPad");
        //For Test
        List<string> rivalChessList = new List<string>{
            "Card001",
            "Card002", "Card002",
            "Card003", "Card003"
        };
        List<string> chessPool = new List<string>{
            "Card001", "Card001", "Card001", "Card001", "Card001",
            "Card002", "Card002", "Card002", "Card002", "Card002",
            "Card003", "Card003", "Card003", "Card003", "Card003",
        };
        InitGame(chessPool, rivalChessList);
    }

    void InitGame(List<string> chessPool, List<string> rivalChessList) {
        gameStatus = GameStatus.INIT;
        InitChessPadStandard();
        InitChessPoolAndChessSelector(chessPool, 5);
        rivalChessListGlabol = rivalChessList;
        gameStatus = GameStatus.REDISPENSE_CHESS;
        // TODO
        gameStatus = GameStatus.GAMING;
    }

    void InitChessPadStandard() {
        GlobalScope.chessGridStatus = new List<List<int>>{
            new List<int> { 1, 10, 10, 10, 11 },
            new List<int> { 1, 10, 10, 10, 11 },
            new List<int> { 1, 10, 10, 10, 11 },
        };
        CommitChessStatusToChessPad();
    }

    void InitChessPoolAndChessSelector(List<string> chessPoolConfig, int chessSelectorSize) {
        ChessDispenser.chessPool = chessPoolConfig;
        for( int i = 0 ; i < chessSelectorSize ; i++ ) {
            ChessSelector.PushBackChess(chessDispenser.InstantiateChess(ChessDispenser.DispenseChess()).GetComponent<Chess>());
        }
    }
//----------------------------------------------------------------------------------------------------------------------------------RunGame
    void Update() {
        // Debug.Log($"TheGameTurns: {GameTurns} , The PlayerTurn: {PlayerTurn}");
        if (gameStatus == GameStatus.GAMING) {
            RunGameTurns(rivalChessListGlabol);
        }
    }
    float startTime;
    void RunGameTurns(List<string> rivalChessList) {
        if(gameStatus == GameStatus.GAMING) {
            if(GameTurns % 2 == 0) {
                PlayerTurn = true;
            } else {
                // DoARivalTurn(rivalChessList);
                if (PlayerTurn) {
                    PlayerTurn = false;
                    startTime = Time.time;
                }
                float elapsedTime = Time.time - startTime;
                // Debug.Log(elapsedTime);
                if (elapsedTime > 1.5f) {
                    DoARivalTurn(rivalChessList);
                }
            }
        }
    }

    void DoARivalTurn(List<string> chessInHand) {
        if(chessInHand.Count != 0) {
            GlobalScope.chessGridStatus = Rival.DoRivalInput(GlobalScope.chessGridStatus, chessInHand);
            CommitChessStatusToChessPad();
        }
        GameTurns++;
    }

    public static void CommitChessStatusToChessPad(){
        for(int i = 0; i < GlobalScope.chessPositionNameList.GetLength(0); i++) {
            for(int j = 0; j < GlobalScope.chessPositionNameList.GetLength(1); j++) {
                Transform chessGrid = chessPad.transform.Find(GlobalScope.chessPositionNameList[i, j]);
                if(Enum.IsDefined(typeof(GlobalScope.ChessPosStatus), GlobalScope.chessGridStatus[i][j]) && chessGrid != null) {
                    chessGrid.GetComponent<ChessGrid>().posStatus = (GlobalScope.ChessPosStatus)GlobalScope.chessGridStatus[i][j];
                }
            }
        }
    }
}
