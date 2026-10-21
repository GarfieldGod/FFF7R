namespace FFF7R
{
    /// <summary>对局整体状态。每帧都可能被查询，做成值类型避免分配。</summary>
    public readonly struct GameStateView
    {
        public MatchStatus Status { get; }

        /// <summary>当前该谁落子；未开始时为 <see cref="PlayerSeat.None"/>。</summary>
        public PlayerSeat Current { get; }

        /// <summary>已完成的回合数。</summary>
        public int Turns { get; }

        /// <summary>胜方；未结束或平局为 <see cref="PlayerSeat.None"/>。</summary>
        public PlayerSeat Winner { get; }

        public GameStateView(MatchStatus status, PlayerSeat current, int turns, PlayerSeat winner)
        {
            Status = status;
            Current = current;
            Turns = turns;
            Winner = winner;
        }

        public bool IsGaming => Status == MatchStatus.Gaming;

        public bool IsOver => Status == MatchStatus.GameOver;

        public override string ToString()
            => Status + " cur:" + Current + " turn:" + Turns + " win:" + Winner;
    }
}
