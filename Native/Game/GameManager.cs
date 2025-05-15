using System.Collections.Generic;

public enum GameStatus {
    INIT,
    DISPENSE_CHESS,
    REDISPENSE_CHESS,
    GAMING,
    GAME_END,
    COMPUTE_RESULT,
    GAME_OVER
}

public struct SingleGameConfig {
    public int rivalLevel;
    public List<string> playerChessPool;
    public List<string> rivalChessPool;
    public SingleGameConfig(int rivalLevel, List<string> playerChessPool, List<string> rivalChessPool) {
        this.rivalLevel = rivalLevel;
        this.playerChessPool = playerChessPool;
        this.rivalChessPool = rivalChessPool;
    }
}

public readonly struct Gamer {
    private readonly Dispenser dispenser;
    private readonly Selector selector;
    private readonly Inputer inputer;
    public Gamer(Dispenser dispenser, Selector selector, Inputer inputer) {
        this.dispenser = dispenser;
        this.selector = selector;
        this.inputer = inputer;
    }
    public void InitDespense(int initNum) {
        while(initNum > 0) {
            GetCardFromCardPool();
            initNum--;
        }
    }
    public void GetCardFromCardPool() {
        selector.PushBack(dispenser.Dispense());
    }
    public bool CanInput() {
        return inputer.CanInput();
    }
    public List<Chess> GetChessInHand() {
        return selector.GetAllChess();
    }
    public ChessPad GetChessPad() {
        return inputer.GetChessPad();
    }
    public int Select(Chess chess) {
        int index = selector.GetIndex(chess);
        if (index != -1) {
            selector.Preview(index);
        }
        return index;
    }
    public Chess Select(int index) {
        Chess selectedChess = selector.GetChess(index);
        if (selectedChess != null) {
            selector.Preview(index);
        }
        return selectedChess;
    }
    public void RestoreSelect() {
        selector.CancelPreview();
    }
    public bool AddInput(Input input) {
        return inputer.AddInput(input);
    }
    public void RestoreInput() {
        RestoreSelect();
        inputer.RestoreInput();
    }
    public void CommitInput() {
        inputer.CommitInput();
        selector.Commit();
        GetCardFromCardPool();
    }
}

public class Game
{
    public int turnTimeLeft_;
    public string turnInfoText_;
    public GameStatus gameStatus_ = GameStatus.INIT;
    public int GameTurns = 0;
    public bool playerTurn_ = true;
//----------------------------------------------------------------------------------------------------------------------------------INIT
    protected readonly ChessPad chessPad_ = new ChessPad();
    protected  Gamer player_;
    protected  Gamer rival_;

    public Game(SingleGameConfig singleGameConfig){
        StartSingleGame(singleGameConfig);
    }

    public void StartSingleGame (SingleGameConfig singleGameConfig) {
        InitGame(singleGameConfig.playerChessPool, singleGameConfig.rivalChessPool, InitChessPadStandard());
    }

    void StartMultiPlayerGame () {}

    Gamer InitGamer(InputerType inputerType, List<string> chessPool, List<Chess> chesses = null) {
        Dispenser dispenser = new Dispenser(chessPool);
        chesses ??= new List<Chess>{};
        Selector selector = new Selector(chesses);
        Inputer inputer = new Inputer(dispenser, selector, chessPad_, inputerType);
        return new Gamer(dispenser, selector, inputer);
    }

    void InitGame(List<string> playerChessPool, List<string> rivalChessPool, ChessPad chessPad) {
        gameStatus_ = GameStatus.INIT;
        chessPad_.Copy(chessPad);
        player_ = InitGamer(InputerType.PLAYER, playerChessPool);
        rival_ = InitGamer(InputerType.AI, rivalChessPool);

        gameStatus_ = GameStatus.DISPENSE_CHESS;
        player_.InitDespense(5);
        rival_.InitDespense(5);

        gameStatus_ = GameStatus.REDISPENSE_CHESS;
        // TODO
        gameStatus_ = GameStatus.GAMING;
    }

    void EndGame() {
    }

    public virtual ChessPad InitChessPadStandard() {
        return new ChessPad(
            new List<List<int>>{
                new List<int> { 1, 10, 10, 10, 11 },
                new List<int> { 1, 10, 10, 10, 11 },
                new List<int> { 1, 10, 10, 10, 11 },
            },
            new List<List<Chess>>{
                new List<Chess> { null, null, null, null, null },
                new List<Chess> { null, null, null, null, null },
                new List<Chess> { null, null, null, null, null },
            }
        );
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
            
            // RunGameTurns(30);
        }
        if (Rival.GetAllFriendEmptyGrids(chessPad_.GetChessGridStatus()).Count == 0
            && Rival.GetAllFriendEmptyGrids(Rival.GetChessPosStatusInRivalView(chessPad_.GetChessGridStatus())).Count == 0) {
            gameStatus_ = GameStatus.GAME_END;
        }
    }
    void GameEndStage() {
        if (gameStatus_ == GameStatus.GAME_END) {
            // TODO
            gameStatus_ = GameStatus.COMPUTE_RESULT;
        }
    }
    void ComputeResult() {
        if (gameStatus_ == GameStatus.COMPUTE_RESULT) {
            turnInfoText_ = "游戏结束！";
            turnTimeLeft_ = 0;
        }
    }
//----------------------------------------------------------------------------------------------------------------------------------GAMING : Turns
    private float thisTurnStartTime;
    public virtual int TurnTimeCounter(int turnTime) {
        // int thisTurnTime = (int)(Time.time - thisTurnStartTime);
        // int thisTurnTimeRemaining = turnTime - thisTurnTime;
        // if (thisTurnTime > turnTime) {
        //     NextTurn();
        // }
        // return thisTurnTimeRemaining;
        return 0;
    }
    public virtual void ShowTurnTextInfo(string info) {
        // turnInfoText.fontSize = (int)Mathf.Lerp(200, 60, (Time.time - thisTurnStartTime) * 3);
        // turnInfoText.text = info;
    }
    public virtual void ShowTurnsTimeLeft(int TimeEveryTurn) {
        // turnCountText.text = TurnTimeCounter(TimeEveryTurn).ToString();
    }
    public virtual void NextTurn() {

        // thisTurnStartTime = Time.time;
        // turnInfoText.fontSize = 60;
        GameTurns++;
    }
    public virtual void AiTurn() {
        // int delayTime = 3;
        // if (!rivalInputer_.IfCanDoInput()) {
        //     delayTime = 1;
        // }
        // if (Time.time - thisTurnStartTime > delayTime) {
        //     ChessInputParms chessInputParms = AiRival.GetTheBestInput(ChessPad.chessPadInfo_, rivalInputer_.GetChessInChessInHand());
        //     if (rivalInputer_.IfCanDoInput() && !chessInputParms.Empty()) {
        //         Log.TestLine("chessInputParms.Empty(): False");
        //         ChessInputParm chessInputParm = new ChessInputParm(
        //             chessInputParms,
        //             ChessPad.chessPadInfo_
        //         );
        //         rivalInputer_.GetChessInput(chessInputParm.chessInputParms.pos, chessInputParm.chessInputParms.cardCode, chessInputParm.chessPadInfo);
        //         ChessPad.CommitChessPadInfoToChessPad(ChessPad.chessPadInfo_);
        //     }
        //     NextTurn();
        // }
    }
    public virtual void RivalTurn() {
        NextTurn();
    }
    public virtual void PlayerTurn() {
        // if (playerInputer_.IfCanDoInput()) {
        //     playerInputer_.GetChessInput(parmsInput.chessGrid, parmsInput.chessObj, parmsInput.chessPadInfo);
        //     ChessPad.CommitChessPadInfoToChessPad(ChessPad.chessPadInfo_);
        // }
        NextTurn();
    }
    public virtual void RunGameTurns(int TimeEveryTurn) {
        ShowTurnsTimeLeft(TimeEveryTurn);
        if (gameStatus_ == GameStatus.GAMING) {
            if (GameTurns % 2 == 0) {
                ShowTurnTextInfo("Your Turn!");
                playerTurn_ = true;

                if (!player_.CanInput()) {
                    ShowTurnTextInfo("No Input Can Do!");
                    NextTurn();
                }
                PlayerTurn();
            } else {
                ShowTurnTextInfo("Rival Turn!");
                playerTurn_ = false;

                if (!rival_.CanInput()) {
                    ShowTurnTextInfo("No Input Can Do!");
                    NextTurn();
                }
                RivalTurn();
            }
        }
    }
//----------------------------------------------------------------------------------------------------------------------------------COMPUTE_RESULT
    public static int GetScoreInOneLine(List<List<List<int>>> originChessStatus, int Line) {
        int score = 0;
        for(int j = 0; j< originChessStatus[0].Count; j++) {
            if (originChessStatus[0][Line][j] == (int)ChessPosStatus.OCCUPIED_FRIEND) {
                score += originChessStatus[1][Line][j] + originChessStatus[2][Line][j];
            }
        }
        return score;
    }
    // public static Tuple<int, int> GetGameResult(List<List<List<int>>> chessGridStatus) {
    //     int playerScoreFinal = 0;
    //     int rivalScoreFinal = 0;
    //     for(int i = 0; i < GlobalScope.chessGridNameList_.Count; i++) {
    //         int playerScore = Rival.GetScoreInOneLine(chessGridStatus, i);
    //         int rivalScore = Rival.GetScoreInOneLine(Rival.GetGridLevelInRivalView(chessGridStatus), i);
    //         playerScoreFinal += playerScore;
    //         rivalScoreFinal += rivalScore;
    //     }
    //     return new Tuple<int, int>(playerScoreFinal, rivalScoreFinal);
    // }
//----------------------------------------------------------------------------------------------------------------------------------GAME_OVER
// TODO
    public static void RepeatTurns() {
        string input = "";
        while(input != "end") {

        }
    }
}