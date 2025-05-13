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

public struct Gamer {
    private Dispenser dispenser;
    private Selector selector;
    private Inputer inputer;
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
}

public class Game
{
    public int turnTimeLeft_;
    public string turnInfoText_;
    public GameStatus gameStatus_ = GameStatus.INIT;
    public int GameTurns = 0;
    public bool PlayerTurn = true;
//----------------------------------------------------------------------------------------------------------------------------------INIT
    private ChessPad chessPad_;
    private Gamer player_;
    private Gamer rival_;

    public Game(SingleGameConfig singleGameConfig){
        StartSingleGame(singleGameConfig);
    }

    void Start() {
        // StartSingleGame(singleGameConfig);
    }

    void StartSingleGame (SingleGameConfig singleGameConfig) {
        InitGame(singleGameConfig.playerChessPool, singleGameConfig.rivalChessPool);
    }

    void StartMultiPlayerGame () {}

    Gamer InitGamer(InputerType inputerType, List<string> chessPool, List<Chess> chesses = null) {
        Dispenser dispenser = new Dispenser(chessPool);
        if(chesses == null) {
            chesses = new List<Chess>{};
        }
        Selector selector = new Selector(chesses);
        Inputer inputer = new Inputer(dispenser, selector, chessPad_, inputerType);
        return new Gamer(dispenser, selector, inputer);
    }

    void InitGame(List<string> playerChessPool, List<string> rivalChessPool) {
        gameStatus_ = GameStatus.INIT;
        chessPad_ = InitChessPadStandard();
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

    ChessPad InitChessPadStandard() {
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
        if (Rival.GetAllVaildChessGrids(chessPad_.GetChessGridStatus()).Count == 0
            && Rival.GetAllVaildChessGrids(Rival.GetChessPosStatusInRivalView(chessPad_.GetChessGridStatus())).Count == 0) {
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
    // private float thisTurnStartTime;
    // private int TurnTimeCounter(int turnTime) {
    //     int thisTurnTime = (int)(Time.time - thisTurnStartTime);
    //     int thisTurnTimeRemaining = turnTime - thisTurnTime;
    //     if(thisTurnTime > turnTime) {
    //         NextTurn();
    //     }
    //     return thisTurnTimeRemaining;
    // }
    // private void showTurnTextInfo(string info) {
    //     turnInfoText.fontSize = (int)Mathf.Lerp(200, 60, (Time.time - thisTurnStartTime) * 3);
    //     turnInfoText.text = info;
    // }
    // void RunGameTurns(int TimeEveryTurn) {
    //     turnCountText.text = TurnTimeCounter(TimeEveryTurn).ToString();
    //     if(gameStatus_ == GameStatus.GAMING) {
    //         if(GameTurns % 2 == 0) {
    //             showTurnTextInfo("Your Turn!");
    //             PlayerTurn = true;
    //             if(Rival.GetAllVaildChessGrids(ChessPad.chessPadInfo_.chessPadStatus[0]).Count == 0) {
    //                 NextTurn();
    //             }
    //         } else {
    //             showTurnTextInfo("Rival Turn!");
    //             if (PlayerTurn) {
    //                 PlayerTurn = false;
    //             }
    //             int delayTime = 3;
    //             if(!rivalInputer_.IfCanDoInput()) {
    //                 delayTime = 1;
    //             }
    //             if (Time.time - thisTurnStartTime > delayTime) {
    //                 DoAiRivalTurn();
    //             }
    //         }
    //     }
    // }
    // public void NextTurn() {
    //     thisTurnStartTime = Time.time;
    //     turnInfoText.fontSize = 60;
    //     GameTurns++;
    // }
    // public void DoAiRivalTurn() {
    //     ChessInputParms chessInputParms = AiRival.GetTheBestInput(ChessPad.chessPadInfo_, rivalInputer_.GetChessInChessInHand());
    //     if(rivalInputer_.IfCanDoInput() && !chessInputParms.Empty()) {
    //         Log.test("chessInputParms.Empty(): False");
    //         ChessInputParm chessInputParm = new ChessInputParm(
    //             chessInputParms,
    //             ChessPad.chessPadInfo_
    //         );
    //         rivalInputer_.GetChessInput(chessInputParm.chessInputParms.pos, chessInputParm.chessInputParms.cardCode, chessInputParm.chessPadInfo);
    //         ChessPad.CommitChessPadInfoToChessPad(ChessPad.chessPadInfo_);
    //     }
    //     NextTurn();
    // }
    // public void DoPlayerTurn(ChessInputParmObj parmsInput){
    //     if(playerInputer_.IfCanDoInput()) {
    //         playerInputer_.GetChessInput(parmsInput.chessGrid, parmsInput.chessObj, parmsInput.chessPadInfo);
    //         ChessPad.CommitChessPadInfoToChessPad(ChessPad.chessPadInfo_);
    //     }
    //     NextTurn();
    // }
//----------------------------------------------------------------------------------------------------------------------------------COMPUTE_RESULT
    public static int GetScoreInOneLine(List<List<List<int>>> originChessStatus, int Line) {
        int score = 0;
        for(int j = 0; j< originChessStatus[0].Count; j++) {
            if(originChessStatus[0][Line][j] == (int)ChessPosStatus.OCCUPIED_FRIEND) {
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
    //         int rivalScore = Rival.GetScoreInOneLine(Rival.GetChessPadStatusInRivalView(chessGridStatus), i);
    //         playerScoreFinal += playerScore;
    //         rivalScoreFinal += rivalScore;
    //     }
    //     return new Tuple<int, int>(playerScoreFinal, rivalScoreFinal);
    // }
//----------------------------------------------------------------------------------------------------------------------------------GAME_OVER
// TODO
    public static void repeatTurns() {
        string input = "";
        while(input != "end") {

        }
    }

    // public static void ShowChessInChessSelector() {
    //     Console.WriteLine("Cards:");
    //     foreach (var chess in ChessSelector.chessList) {
    //         Console.Write(" ");
    //         Console.Write(chess.level);
    //     }
    //     Console.Write("\n");
    // }

    // public static void ShowChessGrid() {
    //     Console.WriteLine("The ChessGrid:");
    //     foreach(List<int> gridLine in chessGridStatus) {
    //         foreach(int grid in gridLine) {
    //             Console.Write(grid.ToString() + " ");
    //         }
    //         Console.Write("\n");
    //     }
    // }
}