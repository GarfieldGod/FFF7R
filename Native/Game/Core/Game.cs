namespace FFF7RCore {
    using System;
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

        /// <summary>
        /// 牌序种子。0 表示不指定（由 <see cref="Game"/> 随机生成并可从 <see cref="Game.Seed"/> 读回）。
        /// 同一个种子 + 同一串输入 = 完全相同的对局，用于复现与回放。
        /// </summary>
        public int seed;

        public GameConfig(List<string> playerChessPool, List<string> rivalChessPool, ChessPad chessPad, int seed = 0) {
            this.playerChessPool = playerChessPool;
            this.rivalChessPool = rivalChessPool;
            this.chessPad = chessPad;
            this.seed = seed;
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

        /// <summary>
        /// 本局实际使用的牌序种子。<see cref="GameConfig.seed"/> 为 0 时这里给出随机生成的值，
        /// 记下它就能用同一个种子重放这一局（崩溃复现、回放、录像都靠它）。
        /// </summary>
        public int Seed => seed_;

        private readonly int seed_;
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
            // 0 视为“未指定”，用随机种子并记下来，保证这一局永远可重放
            seed_ = GameConfig.seed != 0 ? GameConfig.seed : new Random().Next(1, int.MaxValue);
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
            // 双方用同一种子派生出各自独立的随机源：两边牌序互不影响，
            // 也不会因为“先抽玩家还是先抽对手”而改变结果。
            int salt = playerType == PlayerType.PLAYER ? 0x2F6E2B1 : 0x1B56C4E9;
            Random random = new Random(unchecked(seed_ * 31 + salt));
            Dispenser dispenser = new Dispenser(chessPool, random);
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
            // 必须在 ComputeResult 之后：gameWinner 在那里才被算出
            chessPad_?.eventSystem?.RaiseGameEnd(new EventSystem.GameEndedEventArgs(gameWinner, gameTurns_));
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

            if (!IgnoreTurnLimit && currentPlayer_ != input.playerType) {
                Log.TestLine($"{input.playerType} is not allowed to input, this turn is {currentPlayer_}", TextColor.RED);
                return false;
            }

            return chessPadManager_.CheckInput(input);
        }

        public virtual void NextTurn() {
            PlayerType endedPlayer = currentPlayer_;
            currentPlayer_ = currentPlayer_ == PlayerType.PLAYER ? PlayerType.RIVAL : PlayerType.PLAYER;
            gameTurns_++;
            chessPad_?.eventSystem?.RaiseTurnEnd(new EventSystem.TurnEndedEventArgs(endedPlayer, currentPlayer_, gameTurns_));
        }
    }
}