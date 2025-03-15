using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public Text turnCountText;
    public Text turnInfoText;
    public enum GameStatus {
        INIT,
        REDISPENSE_CHESS,
        GAMING,
        COMPUTE_RESULT,
        GAME_OVER
    }
    public static GameStatus gameStatus_ = GameStatus.INIT;
    public static int GameTurns = 0;
    public bool PlayerTurn = true;
    List<string> rivalChessListGlabol;
    public static Dictionary<Int2D, List<Tuple<Int2D, int>>> playerLastingTasks = new Dictionary<Int2D, List<Tuple<Int2D, int>>>{};
    public static Dictionary<Int2D, List<Tuple<Int2D, int>>> RivalLastingTasks = new Dictionary<Int2D, List<Tuple<Int2D, int>>>{};
//----------------------------------------------------------------------------------------------------------------------------------InitGame
    void Start()
    {
        SingleGameConfig singleGameConfig = new SingleGameConfig(
            1,
            new List<string>{
                "Card001", "Card001", "Card001",
                "Card007", "Card007", "Card007", "Card007",
                "Card008", "Card008", "Card008", "Card008",
                "Card009", "Card009", "Card009", "Card009",
            },
            new List<string>{
                "Card001", "Card001", "Card001",
                "Card007", "Card007", "Card007", "Card007",
                "Card008", "Card008", "Card008", "Card008",
                "Card009", "Card009", "Card009", "Card009",
            }
        );
        StartSingleGame(singleGameConfig);
    }
    void StartSingleGame (SingleGameConfig singleGameConfig) {
        InitGame(singleGameConfig.playerChessPool, singleGameConfig.rivalChessPool);
    }
    void StartMultiPlayerGame () {}
    void InitGame(List<string> playerChessPool, List<string> rivalChessPool) {
        gameStatus_ = GameStatus.INIT;
        InitChessPadStandard();
        InitChessPoolAndChessSelector(playerChessPool, 5);
        rivalChessListGlabol = rivalChessPool;
        gameStatus_ = GameStatus.REDISPENSE_CHESS;
        // TODO
        gameStatus_ = GameStatus.GAMING;
    }
    void EndGame() {
        playerLastingTasks.Clear();
        RivalLastingTasks.Clear();
    }

    void InitChessPadStandard() {
        GlobalScope.chessGridStatus = new List<List<List<int>>> {
            new List<List<int>>{
                new List<int> { 1, 10, 10, 10, 11 },
                new List<int> { 1, 10, 10, 10, 11 },
                new List<int> { 1, 10, 10, 10, 11 },
            },
            new List<List<int>>{
                new List<int> { 0, 0, 0, 0, 0 },
                new List<int> { 0, 0, 0, 0, 0 },
                new List<int> { 0, 0, 0, 0, 0 },
            }
        };
        CommitChessStatusToChessPad();
    }

    void InitChessPoolAndChessSelector(List<string> chessPoolConfig, int chessSelectorSize) {
        ChessDispenser.chessPool = chessPoolConfig;
        for( int i = 0 ; i < chessSelectorSize ; i++ ) {
            ChessSelector.PushBackChess(ChessDispenser.InstantiateChess(ChessDispenser.DispenseChess()).GetComponent<Chess>());
        }
    }
//----------------------------------------------------------------------------------------------------------------------------------RunGame
    void Update() {
        // Debug.Log($"TheGameTurns: {GameTurns} , The PlayerTurn: {PlayerTurn}");
        GameRuning();
        ComputeResult();
    }
    void GameRuning() {
        if (gameStatus_ == GameStatus.GAMING) {
            RunGameTurns(rivalChessListGlabol, 30);
        }
        if (Rival.GetAllVaildChessGrids(GlobalScope.chessGridStatus[0]).Count == 0
            && Rival.GetAllVaildChessGrids(Rival.GetChessStatusInRivalView(GlobalScope.chessGridStatus)[0]).Count == 0) {
            gameStatus_ = GameStatus.COMPUTE_RESULT;
        }
    }
    void ComputeResult() {
        if (gameStatus_ == GameStatus.COMPUTE_RESULT) {
            turnInfoText.text = "游戏结束！";
            turnInfoText.fontSize = 60;
        }
    }
    public static void CommitChessStatusToChessPad(){
        List<GameObject> chessGrids = GlobalScope.GetAllChessGrid();
        foreach(var chessGridObj in chessGrids) {
            ChessGrid chessGrid = chessGridObj.GetComponent<ChessGrid>();
            chessGrid.UpdateGridStatus(GlobalScope.chessGridStatus);
        }
    }
//----------------------------------------------------------------------------------------------------------------------------------GameTurn
    private float thisTurnStartTime;
    private int TurnTimeCounter(int turnTime) {
        int thisTurnTime = (int)(Time.time - thisTurnStartTime);
        int thisTurnTimeRemaining = turnTime - thisTurnTime;
        if(thisTurnTime > turnTime) {
            NextTurn();
        }
        return thisTurnTimeRemaining;
    }
    private void showTurnTextInfo(string info) {
        turnInfoText.fontSize = (int)Mathf.Lerp(200, 60, (Time.time - thisTurnStartTime) * 3);
        turnInfoText.text = info;
    }
    void RunGameTurns(List<string> rivalChessList, int TimeEveryTurn) {
        turnCountText.text = TurnTimeCounter(TimeEveryTurn).ToString();
        if(gameStatus_ == GameStatus.GAMING) {
            if(GameTurns % 2 == 0) {
                showTurnTextInfo("Your Turn!");
                PlayerTurn = true;
                if(Rival.GetAllVaildChessGrids(GlobalScope.chessGridStatus[0]).Count == 0) {
                    NextTurn();
                }
            } else {
                showTurnTextInfo("Rival Turn!");
                if (PlayerTurn) {
                    PlayerTurn = false;
                }
                int delayTime = 3;
                if(rivalChessList.Count == 0) {
                    delayTime = 1;
                }
                if (Time.time - thisTurnStartTime > delayTime) {
                    DoAiRivalTurn(rivalChessList);
                }
            }
        }
    }
    public void NextTurn() {
        thisTurnStartTime = Time.time;
        turnInfoText.fontSize = 60;
        GameTurns++;
    }
    public void DoAiRivalTurn(List<string> chessInHand) {
        if(chessInHand.Count != 0) {
            GlobalScope.chessGridStatus = AiRival.DoAiRivalInput(GlobalScope.chessGridStatus, chessInHand, RivalLastingTasks);
            CommitChessStatusToChessPad();
        }
        NextTurn();
    }
    public void DoPlayerTurn(ChessInputParmObj parmsInput){
        ChessInputer.GetChessInput(parmsInput);
        CommitChessStatusToChessPad();
        NextTurn();
    }
}