namespace FFF7R.Adapter
{
    using System;
    using System.Collections.Generic;
    using FFF7RCore;
    using FFF7RCore.Effect;

    /// <summary>
    /// <see cref="IGameSession"/> 的本地实现：把一个 <see cref="FFF7RCore.Game"/> 包成纯契约。
    ///
    /// 表现层只需要 <c>new LocalGameSession(setup)</c>，之后拿到的全是 <c>FFF7R</c> 命名空间下的类型，
    /// 看不到 Game / ChessPad / ChessProperty / Int2D 中的任何一个。
    /// </summary>
    public sealed class LocalGameSession : IGameSession
    {
        private readonly Game game_;
        private readonly CardCatalog catalog_;
        private readonly SessionEventBridge bridge_;
        private readonly int turnLimit_;
        private bool disposed_;

        private Action<GameEvent> raised_;

        public ICardCatalog Catalog => catalog_;

        public int Seed => game_.Seed;

        public LocalGameSession(GameSetup setup)
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
                setup.Seed);

            game_ = new Game(config);
            game_.Init();

            turnLimit_ = setup.TurnLimit;

            // Init 内部 new 了 ChessPad 与 ChessPadManager，eventSystem 这时才存在
            bridge_ = new SessionEventBridge(game_.ChessPad.eventSystem, OnBridgeEvent);
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

        //-------------------------------------------------------------------- IGameCommand

        public CommandResult Start(PlayerSeat firstPlayer)
        {
            if (disposed_) return CommandResult.Fail(RejectReason.Unknown);

            // 内核 Init 之后停在“重抽”阶段，而 ReDispense 目前是空实现，直接推进到 Ready
            if (game_.Status == GameStatus.REDISPENSE_CHESS)
            {
                game_.ReDispense(new List<int>(), new List<int>());
            }

            if (game_.Status != GameStatus.Ready) return CommandResult.Fail(RejectReason.WrongPhase);

            bridge_.ResetCount();
            game_.Start(firstPlayer.ToKernel());
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
            if (!game_.Input(input))
            {
                // 走到这里说明内核在 CheckInput 之后还有别的拒绝条件，契约层无法细分
                return CommandResult.Fail(RejectReason.IllegalPlacement);
            }

            ApplyTurnLimit();
            return CommandResult.Success(bridge_.RaisedCount);
        }

        public CommandResult ReDispense(PlayerSeat seat, IReadOnlyList<int> keepIndexes)
        {
            if (disposed_) return CommandResult.Fail(RejectReason.Unknown);
            if (game_.Status != GameStatus.REDISPENSE_CHESS) return CommandResult.Fail(RejectReason.WrongPhase);

            List<int> kept = keepIndexes != null ? new List<int>(keepIndexes) : new List<int>();
            List<int> none = new List<int>();

            bridge_.ResetCount();
            if (seat == PlayerSeat.Rival) game_.ReDispense(none, kept);
            else game_.ReDispense(kept, none);

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

            Input candidate = new Input(kernelPos, property, kernelSeat);
            if (!game_.CheckInput(candidate))
            {
                PosStatus status = game_.ChessPad[kernelPos.x, kernelPos.y].Status;
                // 费用 > 0 的牌只能落在空/低级格上；落在已占据格是最常见的一种“合法拒绝”
                reason = property.Cost > 0 && status >= PosStatus.OCCUPIED_PLAYER
                    ? RejectReason.CellOccupied
                    : RejectReason.IllegalPlacement;
                return false;
            }

            input = candidate;
            reason = RejectReason.None;
            return true;
        }

        /// <summary>
        /// 回合上限由会话层负责收尾。
        /// 内核自己的 <c>Game.HasNextTurn()</c> 目前在 GAMING 期间恒为 true，
        /// 导致 <c>Input</c> 里那句 <c>if (!HasNextTurn()) End();</c> 永不成立，GameEnded 打不出来。
        /// 在把它修好之前，这里用配置的回合上限主动 End，保证这条链路可测。
        /// </summary>
        private void ApplyTurnLimit()
        {
            if (turnLimit_ <= 0) return;
            if (game_.Status != GameStatus.GAMING) return;
            if (game_.Turns < turnLimit_) return;
            game_.End();
        }

        public void Dispose()
        {
            if (disposed_) return;
            disposed_ = true;

            bridge_.Dispose();
            if (game_.ChessPad != null && game_.ChessPad.eventSystem != null)
            {
                game_.ChessPad.eventSystem.ClearAll();
            }
        }
    }
}
