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

        /// <summary>
        /// 回合上限（以 <see cref="Game.Turns"/> 计）。0 表示不按回合数结束。
        /// 注意：即使为 0，双方手牌都抽空时对局也会结束（见 <see cref="Game.HasNextTurn"/>）。
        /// </summary>
        public int turnLimit;

        public GameConfig(List<string> playerChessPool, List<string> rivalChessPool, ChessPad chessPad, int seed = 0, int turnLimit = 0) {
            this.playerChessPool = playerChessPool;
            this.rivalChessPool = rivalChessPool;
            this.chessPad = chessPad;
            this.seed = seed;
            this.turnLimit = turnLimit;
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

        /// <summary>回合上限，0 表示只按"双方无牌可出"结束。<see cref="GameConfig.turnLimit"/>。</summary>
        public int TurnLimit => turnLimit_;

        private readonly int seed_;
        private readonly int turnLimit_;
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
            turnLimit_ = GameConfig.turnLimit;
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

            // 第一手就可能全手牌都下不去（例如起手 5 张费用 3，场上只有 1 级格）。
            // 没有这一步，客户端一开局就会"轮到你却点不动任何格子"。
            EnsurePlayableTurn();
        }
    //----------------------------------------------------------------------------------------------------------------------------------End
        /// <summary>
        /// 真实对局规则：动不了的一方自动跳过，双方都动不了就结束。
        /// 脚本化测试用例（<see cref="Test.TestGame"/>）没有手牌、棋盘也是固定摆好的，
        /// 必须关掉它，否则第一步之后就会被判定结束。
        /// </summary>
        protected virtual bool UseRealMatchRules => true;

        /// <summary>
        /// 对局是否还能继续。三个终止条件：
        /// <list type="number">
        /// <item>达到回合上限（<see cref="GameConfig.turnLimit"/> &gt; 0 时才生效）；</item>
        /// <item>双方手牌都已抽空 —— 补牌是即时的，手牌空等价于牌池也空，即任何一方都动不了；</item>
        /// <item>双方都没有任何合法落子（棋盘被铺满且地形等级都不够，或手里只剩下不去的牌）。</item>
        /// </list>
        /// </summary>
        public virtual bool HasNextTurn()
        {
            if (gameStatus_ != GameStatus.GAMING) return false;
            if (!UseRealMatchRules) return true;

            if (turnLimit_ > 0 && gameTurns_ >= turnLimit_) return false;

            if (!player_.CanInput && !rival_.CanInput) return false;

            if (!HasAnyLegalMove(PlayerType.PLAYER) && !HasAnyLegalMove(PlayerType.RIVAL)) return false;

            return true;
        }

        /// <summary>
        /// 这一方手里有没有任何一张牌能落在任何一格上。
        /// 只用来判定"还能不能行动"，因此不理会当前该谁走。
        /// </summary>
        private bool HasAnyLegalMove(PlayerType playerType)
        {
            GamePlayer player = playerType == PlayerType.RIVAL ? rival_ : player_;
            if (player == null) return false;

            List<Chess> hand = player.ChessInHand;
            if (hand == null || hand.Count == 0) return false;

            for (int i = 0; i < hand.Count; i++)
            {
                Chess chess = hand[i];
                if (chess == null || chess.Property == null) continue;

                // 用下标遍历而不是 foreach：ChessPad 的枚举器是接口返回，foreach 会装箱。
                // CheckInput 里那条越界日志只在外部传入非法坐标时才会打，这里不会触发。
                List<List<PadGrid>> grid = chessPad_.GridMap;
                for (int x = 0; x < grid.Count; x++)
                {
                    List<PadGrid> line = grid[x];
                    for (int y = 0; y < line.Count; y++)
                    {
                        if (chessPadManager_.CheckInput(new Input(line[y].Position, chess.Property, playerType, i)))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
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
        public virtual bool Input(Input input) => Input(input, out _);

        /// <summary>
        /// 落子。成功后依次做三件事：消耗这张手牌并补牌 → 换边（必要时跳过动不了的一方）→ 判定对局是否该结束。
        /// </summary>
        public virtual bool Input(Input input, out InputReject reject) {
            if (!CheckInput(input, out reject)) return false;

            if (!chessPadManager_.Input(input)) {
                reject = InputReject.Unknown;
                return false;
            }

            ConsumeCard(input);
            AdvanceTurn();

            reject = InputReject.None;
            return true;
        }

        /// <summary>
        /// 换边，并在新回合方没有任何合法落子时自动跳过（pass）。
        /// <para>
        /// 没有这一步，棋盘铺满后行动方会"有手牌但无处可下"，对局就永远停在那里 ——
        /// 客户端表现为轮到你却点不动任何格子。跳过之后两种情况都会收敛：
        /// 另一方还能走就继续，双方都不能走就结束。
        /// </para>
        /// </summary>
        private void AdvanceTurn() {
            NextTurn();
            EnsurePlayableTurn();
        }

        /// <summary>
        /// 保证 <see cref="currentPlayer_"/> 至少有一手能走：走不了就让出回合，
        /// 双方都走不了就结束。需要重复是因为让出一方后接手的另一方也可能动不了。
        /// </summary>
        private void EnsurePlayableTurn() {
            if (!UseRealMatchRules) return;

            while (gameStatus_ == GameStatus.GAMING) {
                if (turnLimit_ > 0 && gameTurns_ >= turnLimit_) { End(); return; }

                if (HasAnyLegalMove(currentPlayer_)) return;   // 正常情况：轮到的人能走

                // 轮到的人动不了：另一方还能走就让出回合，双方都动不了就结束
                if (!HasAnyLegalMove(Other(currentPlayer_))) { End(); return; }

                PassTurn();
            }
        }

        /// <summary>
        /// 跳过动不了的一方。只换边、不推进回合数 —— 否则 pass 会吃掉回合上限，
        /// 让"回合上限 7"实际只够走三四手。
        /// </summary>
        private void PassTurn() {
            PlayerType skipped = currentPlayer_;
            currentPlayer_ = Other(currentPlayer_);
            chessPad_?.eventSystem?.RaiseTurnEnd(
                new EventSystem.TurnEndedEventArgs(skipped, currentPlayer_, gameTurns_, true));
        }

        private static PlayerType Other(PlayerType playerType) {
            return playerType == PlayerType.PLAYER ? PlayerType.RIVAL : PlayerType.PLAYER;
        }

        /// <summary>
        /// 把打出的牌从手牌里丢掉并补一张。只有 <see cref="Input.FromHand"/> 才消耗 ——
        /// 脚本 / 测试直接喂牌的路径（handIndex = -1）不碰手牌。
        /// </summary>
        private void ConsumeCard(Input input) {
            if (!input.FromHand) return;

            GamePlayer player = input.playerType == PlayerType.RIVAL ? rival_ : player_;
            player.DiscardFromHand(input.handIndex);
        }

        public virtual bool CheckInput(Input input) => CheckInput(input, out _);

        public virtual bool CheckInput(Input input, out InputReject reject)
        {
            if (gameStatus_ != GameStatus.GAMING) { reject = InputReject.NotGaming; return false; }

            if (!IgnoreTurnLimit && currentPlayer_ != input.playerType) {
                Log.TestLine($"{input.playerType} is not allowed to input, this turn is {currentPlayer_}", TextColor.RED);
                reject = InputReject.NotYourTurn;
                return false;
            }

            if (input.Empty) { reject = InputReject.EmptyInput; return false; }

            // 由手牌驱动的请求必须能对上手里那一张，否则内核会接受"出不在手里的牌"
            if (input.FromHand && !IsHandIndexValid(input)) {
                reject = InputReject.CardNotInHand;
                return false;
            }

            return chessPadManager_.CheckInput(input, out reject);
        }

        private bool IsHandIndexValid(Input input)
        {
            GamePlayer player = input.playerType == PlayerType.RIVAL ? rival_ : player_;
            if (player == null) return false;

            List<Chess> hand = player.ChessInHand;
            if (hand == null || input.handIndex >= hand.Count) return false;

            Chess chess = hand[input.handIndex];
            return chess != null && chess.Property != null && chess.Property.CardCode == input.chess.CardCode;
        }

        public virtual void NextTurn() {
            PlayerType endedPlayer = currentPlayer_;
            currentPlayer_ = currentPlayer_ == PlayerType.PLAYER ? PlayerType.RIVAL : PlayerType.PLAYER;
            gameTurns_++;
            chessPad_?.eventSystem?.RaiseTurnEnd(new EventSystem.TurnEndedEventArgs(endedPlayer, currentPlayer_, gameTurns_));
        }
    }
}