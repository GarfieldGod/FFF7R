namespace FFF7RCore {
    /// <summary>
    /// 一次落子请求被拒绝的原因。内核只判规则，文案由表现层决定。
    /// </summary>
    public enum InputReject {
        None = 0,

        /// <summary>对局不在 GAMING 阶段。</summary>
        NotGaming,

        /// <summary>不是这一方该走。</summary>
        NotYourTurn,

        /// <summary>没有带牌（<see cref="Input.chess"/> 为 null）。</summary>
        EmptyInput,

        /// <summary>指定的手牌下标不存在，或该下标上的牌与请求的卡号不一致。</summary>
        CardNotInHand,

        /// <summary>目标格不在棋盘内。</summary>
        OutOfBoard,

        /// <summary>费用 &gt; 0 的牌，目标格已被棋子占据。</summary>
        CellOccupied,

        /// <summary>费用 &gt; 0 的牌，目标格的地形等级低于费用。</summary>
        CellLevelTooLow,

        /// <summary>费用 = 0（覆盖型）的牌，目标格上没有己方棋子可供覆盖。</summary>
        NoOwnChessToCover,

        Unknown = 99,
    }

    public struct Input {
        public Int2D pos;
        public ChessProperty chess;
        public PlayerType playerType;

        /// <summary>
        /// 这张牌的来源手牌下标。-1 表示<b>不由手牌驱动</b>（脚本 / 测试直接喂一张牌）。
        /// <para>
        /// 为 -1 时内核既不校验手牌归属，也不在落子成功后消耗手牌 —— 保持脚本化对局的兼容。
        /// 走 <c>IGameCommand.Place</c> 的正常路径一律带真实下标，因此内核能自己保证
        /// ”不能出不在手里的牌“，不再依赖外层约束。
        /// </para>
        /// </summary>
        public int handIndex;

        public Input(Int2D pos, ChessProperty chess, PlayerType playerType) : this(pos, chess, playerType, -1) { }

        public Input(Int2D pos, ChessProperty chess, PlayerType playerType, int handIndex) {
            this.pos = pos;
            this.chess = chess;
            this.playerType = playerType;
            this.handIndex = handIndex;
        }

        public Input(Int2D pos, string cardCode, PlayerType playerType) : this(pos, cardCode, playerType, -1) { }

        public Input(Int2D pos, string cardCode, PlayerType playerType, int handIndex) {
            this.pos = pos;
            this.chess = Property.GetChessProperty(cardCode);
            this.playerType = playerType;
            this.handIndex = handIndex;
        }

        public readonly bool Empty => chess == null;

        /// <summary>是否由手牌驱动（决定内核要不要校验归属、要不要消耗）。</summary>
        public readonly bool FromHand => handIndex >= 0;

        public override string ToString() => $"PlayerType: {playerType} Pos: {pos} Chess: {chess.CardCode} Hand: {handIndex}";
    }
}
