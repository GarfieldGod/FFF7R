namespace FFF7R
{
    /// <summary>指令被拒绝的原因。表现层据此决定提示文案，不需要 parse 字符串。</summary>
    public enum RejectReason
    {
        None = 0,

        /// <summary>对局不在进行中（还没 Start、已结束、或处于重抽阶段）。</summary>
        NotGaming,

        /// <summary>不是这一方该走。</summary>
        NotYourTurn,

        /// <summary>手牌下标越界。</summary>
        HandIndexOutOfRange,

        /// <summary>该下标上的牌与请求的卡不一致（手牌已变化，表现层需刷新）。</summary>
        CardNotInHand,

        /// <summary>卡牌编号查不到定义。</summary>
        CardNotFound,

        /// <summary>目标格不在棋盘内。</summary>
        OutOfBoard,

        /// <summary>目标格已被占据，不能落子。</summary>
        CellOccupied,

        /// <summary>目标格的地形等级低于这张牌的费用。</summary>
        CellLevelTooLow,

        /// <summary>费用为 0 的覆盖型卡，目标格上没有己方棋子可覆盖。</summary>
        NoOwnChessToCover,

        /// <summary>该卡在此处不合法（内核拒绝但未细分原因）。</summary>
        IllegalPlacement,

        /// <summary>当前阶段不接受该指令。</summary>
        WrongPhase,

        Unknown = 99,
    }
}
