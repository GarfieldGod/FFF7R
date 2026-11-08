namespace FFF7R.Adapter
{
    using System;
    using System.Collections.Generic;
    using FFF7RCore;
    using FFF7RCore.Effect;

    /// <summary>
    /// <see cref="IGameSession"/> 的本地实现：把一个 <see cref="FFF7RCore.Game"/> 包成纯契约。
    ///
    /// 表现层只需要 <c>new GameSession(setup)</c>，之后拿到的全是 <c>FFF7R</c> 命名空间下的类型，
    /// 看不到 Game / ChessPad / ChessProperty / Int2D 中的任何一个。
    /// </summary>
    public sealed class GameSession : IGameSession
    {
        private readonly Game game_;
        private readonly CardCatalog catalog_;
        private readonly SessionEventBridge bridge_;
        private bool disposed_;

        private Action<GameEvent> raised_;

        public ICardCatalog Catalog => catalog_;

        public int Seed => game_.Seed;

        public GameSession(GameSetup setup)
        {
            if (setup == null) throw new ArgumentNullException(nameof(setup));

            catalog_ = new CardCatalog();
            catalog_.Load();

            ChessPad pad = new ChessPad(setup.BoardWidth, setup.BoardHeight);
            if (setup.StandardBoard) pad.InitStandard();

            GameConfig config = new GameConfig(
                new List<string>(setup.PlayerPool),
                new List<string>(setup.RivalPool),
                pad,
                setup.Seed,
                setup.TurnLimit);

            game_ = new Game(config);
            game_.Init();

            // Init 内部 new 了 ChessPad 与 ChessPadManager，EventSystem 这时才存在
            bridge_ = new SessionEventBridge(game_.ChessPad.EventSystem, OnBridgeEvent);
        }

        //-------------------------------------------------------------------- IGameEvents

        public long LastSequence => bridge_.LastSequence;

        public event Action<GameEvent> Raised
        {
            add { raised_ += value; }
            remove { raised_ -= value; }
        }

        private void OnBridgeEvent(GameEvent gameEvent) => raised_?.Invoke(gameEvent);

        //-------------------------------------------------------------------- IGameQuery

        public GameStateView State
        {
            get
            {
                MatchStatus status = game_.Status.ToContract();
                return new GameStateView(
                    status,
                    game_.CurrentPlayer.ToContract(),
                    game_.Turns,
                    status == MatchStatus.GameOver ? game_.Winner.ToContract() : PlayerSeat.None);
            }
        }

        public BoardSnapshot Board => KernelMapping.ToBoard(game_.ChessPad);

        public HandSnapshot Hand(PlayerSeat seat)
        {
            if (seat == PlayerSeat.None) return HandSnapshot.Empty;

            List<Chess> hand = seat == PlayerSeat.Player
                ? game_.Player.ChessInHand
                : game_.Rival.ChessInHand;

            if (hand == null || hand.Count == 0) return new HandSnapshot(seat, new CardView[0]);

            CardView[] cards = new CardView[hand.Count];
            for (int i = 0; i < hand.Count; i++)
            {
                Chess chess = hand[i];
                string cardCode = chess != null && chess.Property != null ? chess.Property.CardCode : null;
                cards[i] = new CardView(i, cardCode, catalog_.Get(cardCode));
            }
            return new HandSnapshot(seat, cards);
        }

        public bool CanPlace(PlayerSeat seat, int handIndex, Int2 pos, out RejectReason reason)
        {
            Input input;
            return TryResolve(seat, handIndex, pos, out input, out reason);
        }

        public BoardSnapshot Preview(PlayerSeat seat, int handIndex, Int2 pos)
        {
            Input input;
            RejectReason reason;
            if (!TryResolve(seat, handIndex, pos, out input, out reason)) return null;

            ChessPad previewPad = game_.Preview(input);
            return previewPad == null ? null : KernelMapping.ToBoard(previewPad);
        }

        public bool CanPlaceAny(PlayerSeat seat) => game_.HasAnyLegalMove(seat.ToKernel());

        //-------------------------------------------------------------------- IGameCommand

        public CommandResult Start(PlayerSeat firstPlayer)
        {
            if (disposed_) return CommandResult.Fail(RejectReason.Unknown);

            if (game_.Status == GameStatus.REDISPENSE_CHESS) game_.ReDispense(new List<int>(), new List<int>());
            if (game_.Status != GameStatus.Ready) return CommandResult.Fail(RejectReason.WrongPhase);

            bridge_.ResetCount();
            game_.Start(firstPlayer.ToKernel());
            return CommandResult.Success(bridge_.RaisedCount);
        }

        public CommandResult PassTurn(PlayerSeat seat)
        {
            if (disposed_) return CommandResult.Fail(RejectReason.Unknown);

            if (game_.Status != GameStatus.GAMING) return CommandResult.Fail(RejectReason.WrongPhase);

            bridge_.ResetCount();
            bool ret = game_.PassTurn(seat.ToKernel());
            if (!ret) return CommandResult.Fail(RejectReason.NotYourTurn);

            return CommandResult.Success(bridge_.RaisedCount);
        }

        public CommandResult Place(PlayerSeat seat, int handIndex, Int2 pos)
        {
            if (disposed_) return CommandResult.Fail(RejectReason.Unknown);

            Input input;
            RejectReason reason;
            if (!TryResolve(seat, handIndex, pos, out input, out reason))
            {
                return CommandResult.Fail(reason);
            }

            bridge_.ResetCount();
            InputReject kernelReject;
            if (!game_.Input(input, out kernelReject))
            {
                return CommandResult.Fail(kernelReject.ToContract());
            }

            return CommandResult.Success(bridge_.RaisedCount);
        }

        public CommandResult EndGame()
        {
            if (disposed_) return CommandResult.Fail(RejectReason.Unknown);

            if (game_.Status != GameStatus.GAMING) return CommandResult.Fail(RejectReason.WrongPhase);

            bridge_.ResetCount();
            game_.End();

            return CommandResult.Success(bridge_.RaisedCount);
        }

        public CommandResult ReDispense(List<int> playerDiscardIndexes, List<int> rivalDiscardIndexes)
        {
            if (disposed_) return CommandResult.Fail(RejectReason.Unknown);
            if (game_.Status != GameStatus.REDISPENSE_CHESS) return CommandResult.Fail(RejectReason.WrongPhase);

            bridge_.ResetCount();
            game_.ReDispense(playerDiscardIndexes, rivalDiscardIndexes);

            return CommandResult.Success(bridge_.RaisedCount);
        }

        //-------------------------------------------------------------------- 内部

        /// <summary>
        /// 把一次“出牌请求”解析成内核 Input，并给出人能看懂的拒绝原因。
        /// <see cref="CanPlace"/> / <see cref="Preview"/> / <see cref="Place"/> 共用这一条判定，
        /// 保证“能预览就一定能下”。
        /// </summary>
        private bool TryResolve(PlayerSeat seat, int handIndex, Int2 pos, out Input input, out RejectReason reason)
        {
            input = default;

            if (disposed_)
            {
                reason = RejectReason.Unknown;
                return false;
            }

            if (game_.Status != GameStatus.GAMING)
            {
                reason = RejectReason.NotGaming;
                return false;
            }

            PlayerType kernelSeat = seat.ToKernel();
            if (game_.CurrentPlayer != kernelSeat)
            {
                reason = RejectReason.NotYourTurn;
                return false;
            }

            List<Chess> hand = seat == PlayerSeat.Player
                ? game_.Player.ChessInHand
                : game_.Rival.ChessInHand;

            if (hand == null || handIndex < 0 || handIndex >= hand.Count)
            {
                reason = RejectReason.HandIndexOutOfRange;
                return false;
            }

            Chess chess = hand[handIndex];
            ChessProperty property = chess != null ? chess.Property : null;
            if (property == null)
            {
                reason = RejectReason.CardNotFound;
                return false;
            }

            Int2D kernelPos = pos.ToKernel();
            if (!game_.ChessPad.IsInBoard(kernelPos))
            {
                reason = RejectReason.OutOfBoard;
                return false;
            }

            Input candidate = new Input(kernelPos, property, kernelSeat, handIndex);
            InputReject kernelReject;
            if (!game_.CheckInput(candidate, out kernelReject))
            {
                reason = kernelReject.ToContract();
                return false;
            }

            input = candidate;
            reason = RejectReason.None;
            return true;
        }

        public void Dispose()
        {
            if (disposed_) return;
            disposed_ = true;

            bridge_.Dispose();
            if (game_.ChessPad != null && game_.ChessPad.EventSystem != null)
            {
                game_.ChessPad.EventSystem.ClearAll();
            }
        }
    }
}
