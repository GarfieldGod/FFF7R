namespace FFF7R
{
    using System.Collections.Generic;

    /// <summary>
    /// 棋盘全量快照（某一时刻的完整状态）。
    /// </summary>
    public sealed class BoardSnapshot
    {
        private readonly CellView[] cells_;

        public int Width { get; }
        public int Height { get; }

        /// <summary>按列累计的双方得分（内核 ScoreMap 的合计）。</summary>
        public int PlayerScore { get; }
        public int RivalScore { get; }

        public BoardSnapshot(int width, int height, CellView[] cells, int playerScore, int rivalScore)
        {
            Width = width;
            Height = height;
            cells_ = cells;
            PlayerScore = playerScore;
            RivalScore = rivalScore;
        }

        public bool IsInBoard(Int2 pos) => pos.X >= 0 && pos.X < Width && pos.Y >= 0 && pos.Y < Height;

        /// <summary>按列优先存放，索引 = X * Height + Y。</summary>
        public CellView this[int x, int y] => cells_[x * Height + y];

        public CellView this[Int2 pos] => this[pos.X, pos.Y];

        public IReadOnlyList<CellView> Cells => cells_;

        public override string ToString()
            => Width + "x" + Height + " P" + PlayerScore + " R" + RivalScore;
    }
}
