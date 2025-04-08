using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using Unity.VisualScripting;
using Mirror.Examples.BilliardsPredicted;

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
//----------------------------------------------------------------------------------------------------------------------------------INIT
    private ChessInputer playerInputer_;
    private ChessInputer rivalInputer_;
    void Start()
    {
        SingleGameConfig singleGameConfig = new SingleGameConfig(
            1,
            new List<string>{
                "CardTest0", "CardTest0", "CardTest0", "CardTest0",
                "CardTest1", "CardTest1", "CardTest1", "CardTest1",
                "Card006", "Card006", "Card006", "Card006",
                "Card009", "Card009", "Card009",
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
        playerInputer_ = new ChessInputer(playerChessPool, 5, ChessPad.chessPadInfo_, InputerType.PLAYER);
        rivalInputer_ = new ChessInputer(rivalChessPool, 5, ChessPad.chessPadInfo_, InputerType.RIVAL);

        gameStatus_ = GameStatus.REDISPENSE_CHESS;
        // TODO
        gameStatus_ = GameStatus.GAMING;
    }
    void EndGame() {
    }

    void InitChessPadStandard() {
        ChessPad.chessPadInfo_ = new ChessPadInfo(
        new List<List<List<int>>> {
            new List<List<int>>{
                new List<int> { 1, 10, 10, 10, 11 },
                new List<int> { 1, 10, 10, 10, 11 },
                new List<int> { 1, 10, 10, 10, 11 },
            },
            GlobalScope.DeepCopy2DList(GlobalScope.EmptyStandard2DList),
            GlobalScope.DeepCopy2DList(GlobalScope.EmptyStandard2DList)
        }
        );
        ChessPad.CommitChessPadInfoToChessPad(ChessPad.chessPadInfo_);
    }
//----------------------------------------------------------------------------------------------------------------------------------REDISPENSE_CHESS
// TODO
//----------------------------------------------------------------------------------------------------------------------------------GAMING : RunGame
    void Update() {
        // Debug.Log($"TheGameTurns: {GameTurns} , The PlayerTurn: {PlayerTurn}");
        GameRuning();
        ComputeResult();
    }
    void GameRuning() {
        if (gameStatus_ == GameStatus.GAMING) {
            RunGameTurns(30);
            playerInputer_.GetChessSelector().PreviewChessToPreviewPos();
            rivalInputer_.GetChessSelector().PreviewChessToPreviewPos();
        }
        if (Rival.GetAllVaildChessGrids(ChessPad.chessPadInfo_.chessPadStatus[0]).Count == 0
            && Rival.GetAllVaildChessGrids(Rival.GetChessPadStatusInRivalView(ChessPad.chessPadInfo_.chessPadStatus)[0]).Count == 0) {
            gameStatus_ = GameStatus.COMPUTE_RESULT;
        }
    }
    void ComputeResult() {
        if (gameStatus_ == GameStatus.COMPUTE_RESULT) {
            turnInfoText.text = "游戏结束！";
            turnInfoText.fontSize = 60;
        }
    }
//----------------------------------------------------------------------------------------------------------------------------------GAMING : Turns
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
    void RunGameTurns(int TimeEveryTurn) {
        turnCountText.text = TurnTimeCounter(TimeEveryTurn).ToString();
        if(gameStatus_ == GameStatus.GAMING) {
            if(GameTurns % 2 == 0) {
                showTurnTextInfo("Your Turn!");
                PlayerTurn = true;
                if(Rival.GetAllVaildChessGrids(ChessPad.chessPadInfo_.chessPadStatus[0]).Count == 0) {
                    NextTurn();
                }
            } else {
                showTurnTextInfo("Rival Turn!");
                if (PlayerTurn) {
                    PlayerTurn = false;
                }
                int delayTime = 3;
                if(!rivalInputer_.IfCanDoInput()) {
                    delayTime = 1;
                }
                if (Time.time - thisTurnStartTime > delayTime) {
                    DoAiRivalTurn();
                }
            }
        }
    }
    public void NextTurn() {
        thisTurnStartTime = Time.time;
        turnInfoText.fontSize = 60;
        GameTurns++;
    }
    public void DoAiRivalTurn() {
        ChessInputParms chessInputParms = AiRival.GetTheBestInput(ChessPad.chessPadInfo_, rivalInputer_.GetChessInChessInHand());
        if(rivalInputer_.IfCanDoInput() && !chessInputParms.Empty()) {
            Log.test("chessInputParms.Empty(): False");
            ChessInputParm chessInputParm = new ChessInputParm(
                chessInputParms,
                ChessPad.chessPadInfo_
            );
            rivalInputer_.GetChessInput(chessInputParm.chessInputParms.pos, chessInputParm.chessInputParms.cardCode, chessInputParm.chessPadInfo);
            ChessPad.CommitChessPadInfoToChessPad(ChessPad.chessPadInfo_);
        }
        NextTurn();
    }
    public void DoPlayerTurn(ChessInputParmObj parmsInput){
        if(playerInputer_.IfCanDoInput()) {
            playerInputer_.GetChessInput(parmsInput.chessGrid, parmsInput.chessObj, parmsInput.chessPadInfo);
            ChessPad.CommitChessPadInfoToChessPad(ChessPad.chessPadInfo_);
        }
        NextTurn();
    }
//----------------------------------------------------------------------------------------------------------------------------------COMPUTE_RESULT
    public static Tuple<int, int> GetGameResult(List<List<List<int>>> chessGridStatus) {
        int playerScoreFinal = 0;
        int rivalScoreFinal = 0;
        for(int i = 0; i < GlobalScope.chessGridNameList_.Count; i++) {
            int playerScore = Rival.GetScoreInOneLine(chessGridStatus, i);
            int rivalScore = Rival.GetScoreInOneLine(Rival.GetChessPadStatusInRivalView(chessGridStatus), i);
            playerScoreFinal += playerScore;
            rivalScoreFinal += rivalScore;
        }
        return new Tuple<int, int>(playerScoreFinal, rivalScoreFinal);
    }
//----------------------------------------------------------------------------------------------------------------------------------GAME_OVER
// TODO
    public void DoPreviewToPreViewPos(Chess chess){
        playerInputer_.GetChessSelector().DoPreviewToPreViewPos(chess);
    }
    public void CancelPreview(){
        playerInputer_.GetChessSelector().CancelPreview();
    }
    public void DoPreviewToChessGridPos(GameObject chessGridObj){
        playerInputer_.GetChessSelector().DoPreviewToChessGridPos(chessGridObj);
    }
    public void CancelPreviewToChessGridPos(){
        playerInputer_.GetChessSelector().CancelPreviewToChessGridPos();
    }
}