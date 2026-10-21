namespace FFF7R
{
    /// <summary>
    /// 棋盘单格的只读视图。
    /// <see cref="Level"/> 是叠加 buff 之后的当前等级（表现层显示的数字），
    /// <see cref="BaseLevel"/> 是棋子本身的基础战力（空格为 0）。
    /// </summary>
    public readonly struct CellView
    {
        public Int2 Pos { get; }
        public CellState State { get; }

        /// <summary>格子归属；空格或中立为 <see cref="PlayerSeat.None"/>。</summary>
        public PlayerSeat Owner { get; }

        /// <summary>该格棋子的卡牌编号；空格为 null。</summary>
        public string CardCode { get; }

        /// <summary>含 buff 的当前等级。</summary>
        public int Level { get; }

        /// <summary>不含 buff 的基础战力。</summary>
        public int BaseLevel { get; }

        public CellView(Int2 pos, CellState state, string cardCode, int level, int baseLevel)
        {
            Pos = pos;
            State = state;
            Owner = state.Owner();
            CardCode = cardCode;
            Level = level;
            BaseLevel = baseLevel;
        }

        public bool IsEmpty => CardCode == null;

        public override string ToString()
            => Pos + " " + State + " " + (CardCode ?? "-") + " Lv" + Level;
    }
}
