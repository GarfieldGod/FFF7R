namespace FFF7RCore {
    using System.Collections.Generic;

    public enum PlayerType {
        PLAYER,
        RIVAL,
        Null
    }

    public enum GameStatus {
        INIT,
        DISPENSE_CHESS,
        REDISPENSE_CHESS,
        Ready,
        GAMING,
        GAME_END,
        COMPUTE_RESULT,
        GAME_OVER
    }

    public struct GameConfig {
        public List<string> playerChessPool;
        public List<string> rivalChessPool;
        public ChessPad chessPad;
        public GameConfig(List<string> playerChessPool, List<string> rivalChessPool, ChessPad chessPad) {
            this.playerChessPool = playerChessPool;
            this.rivalChessPool = rivalChessPool;
            this.chessPad = chessPad;
        }
    }

    public class Game
    {
        public GameStatus Status => gameStatus_;
        public PlayerType Winner => gameWinner;
        public int Turns => gameTurns_;
        public PlayerType CurrentPlayer => currentPlayer_;
        public GamePlayer Player => player_;
        public GamePlayer Rival => rival_;
        public ChessPad ChessPad => chessPad_;
        public bool IgnoreTurnLimit = false;
    //----------------------------------------------------------------------------------------------------------------------------------INIT
        protected readonly GameConfig gameConfig_;
        protected ChessPad chessPad_ = null;
        protected ChessPadManager chessPadManager_ = null;
        protected GamePlayer player_ = null;
        protected GamePlayer rival_ = null;
        protected PlayerType gameWinner = PlayerType.Null;
        protected int gameTurns_ = 0;
        protected GameStatus gameStatus_ = GameStatus.INIT;
        protected PlayerType currentPlayer_ = PlayerType.Null;

        public Game(GameConfig GameConfig){
            gameConfig_ = GameConfig;
        }

        public virtual void Init(bool skipReDispense = false) {
            gameStatus_ = GameStatus.INIT;
            chessPad_ = new ChessPad(gameConfig_.chessPad);
            player_ = InitGamePlayer(PlayerType.PLAYER, gameConfig_.playerChessPool);
            rival_ = InitGamePlayer(PlayerType.RIVAL, gameConfig_.rivalChessPool);

            chessPadManager_ = new ChessPadManager(chessPad_);

            gameStatus_ = GameStatus.DISPENSE_CHESS;
            player_.InitDespense(5);
            rival_.InitDespense(5);

            gameStatus_ = skipReDispense? GameStatus.Ready : GameStatus.REDISPENSE_CHESS;
        }

        protected virtual GamePlayer InitGamePlayer(PlayerType playerType, List<string> chessPool, List<Chess> chesses = null) {
            Dispenser dispenser = new Dispenser(chessPool);
            chesses ??= new List<Chess>{};
            Selector selector = new Selector(chesses);
            return new GamePlayer(dispenser, selector, playerType);
        }
    //----------------------------------------------------------------------------------------------------------------------------------ReDispense
        public virtual void ReDispense(List<int> playerIndexes, List<int> rivalIndexes) {
            if (gameStatus_ != GameStatus.REDISPENSE_CHESS) return;

            gameStatus_ = GameStatus.Ready;
        }
    //----------------------------------------------------------------------------------------------------------------------------------Start
        public virtual void Start(PlayerType firstPlayer = PlayerType.PLAYER) {
            if (gameStatus_ != GameStatus.Ready) return;

            currentPlayer_ = firstPlayer;
            gameStatus_ = GameStatus.GAMING;
        }
    //----------------------------------------------------------------------------------------------------------------------------------End
        public virtual bool HasNextTurn()
        {
            if (gameStatus_ != GameStatus.GAMING) return false;

            // if (!player_.CanInput && !rival_.CanInput) {
            //     return false;
            // }

            return true;
        }
        public virtual void End() {
            if (gameStatus_ != GameStatus.GAMING) return;

            gameStatus_ = GameStatus.GAME_END;
            ComputeResult();
        }

        public virtual void ComputeResult()
        {
            if (gameStatus_ != GameStatus.GAME_END) return;

            gameStatus_ = GameStatus.COMPUTE_RESULT;
            int playerScore = 0;
            int rivalScore = 0;
            var ResultList = chessPad_.ScoreMap;
            for (int i = 0; i < ResultList.Count; i++)
            {
                playerScore += ResultList[i][0];
                rivalScore += ResultList[i][1];
            }

            if (playerScore > rivalScore) gameWinner = PlayerType.PLAYER;
            else if (playerScore < rivalScore) gameWinner = PlayerType.RIVAL;
            else gameWinner = PlayerType.Null;

            gameStatus_ = GameStatus.GAME_OVER;
        }
    //----------------------------------------------------------------------------------------------------------------------------------Preview
        public virtual ChessPad Preview(Input input) {
            if (!CheckInput(input)) return null;
            return chessPadManager_.Preview(input);
        }
    //----------------------------------------------------------------------------------------------------------------------------------Input
        public virtual bool Input(Input input) {
            if (!CheckInput(input)) return false;
        
            bool ret = chessPadManager_.Input(input);
            if (ret)
            {
                NextTurn();
                if (!HasNextTurn()) End();
            }

            return ret;
        }

        public virtual bool CheckInput(Input input)
        {
            if (gameStatus_ != GameStatus.GAMING) return false;

            if (!IgnoreTurnLimit && currentPlayer_ != input.playerType) return false;

            return chessPadManager_.CheckInput(input);
        }

        public virtual void NextTurn() {
            currentPlayer_ = currentPlayer_ == PlayerType.PLAYER ? PlayerType.RIVAL : PlayerType.PLAYER;
            gameTurns_++;
        }
    }
}