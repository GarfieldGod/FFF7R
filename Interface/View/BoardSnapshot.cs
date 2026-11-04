namespace FFF7R
{
    using System.Collections.Generic;

    /// <summary>
    /// 棋盘全量快照（某一时刻的完整状态）。
    /// </summary>
    public sealed class BoardSnapshot
    {
        private readonly CellView[] cells_;

        /// <summary>宽度方向（棋盘 y 轴）的格数，是索引器第二个下标的上限。</summary>
        public int Width { get; }

        /// <summary>高度方向（棋盘 x 轴）的格数，是索引器第一个下标的上限。</summary>
        public int Height { get; }

        /// <summary>按列累计的双方得分（内核 ScoreMap 的合计）。</summary>
        public List<int> PlayerScoreList { get; }
        public List<int> RivalScoreList { get; }

        public int PlayerScore
        {
            get
            {
                int ret = 0;
                foreach(var num in PlayerScoreList) {
                    ret += num;
                }
                return ret;
            }
        }

        public int RivalScore
        {
            get
            {
                int ret = 0;
                foreach(var num in RivalScoreList) {
                    ret += num;
                }
                return ret;
            }
        }

        public BoardSnapshot(int width, int height, CellView[] cells, List<int> playerScoreList, List<int> rivalScoreList)
        {
            Width = width;
            Height = height;
            cells_ = cells;
            PlayerScoreList = playerScoreList;
            RivalScoreList = rivalScoreList;
        }

        /// <summary>
        /// 内核坐标是否落在棋盘内。
        /// 注意两个上限是交叉的：x 是高度方向 → 比 <c>Height</c>；y 是宽度方向 → 比 <c>Width</c>。
        /// </summary>
        public bool IsInBoard(Int2 pos) => pos.X >= 0 && pos.X < Height && pos.Y >= 0 && pos.Y < Width;

        /// <summary>
        /// 按 x 主序存放，索引 = X * Width + Y。
        /// 下标范围：<c>x ∈ [0, Height)</c>（高度方向），<c>y ∈ [0, Width)</c>（宽度方向）。
        /// </summary>
        public CellView this[int x, int y] => cells_[x * Width + y];

        public CellView this[Int2 pos] => this[pos.X, pos.Y];

        public IReadOnlyList<CellView> Cells => cells_;

        public override string ToString()
            => Width + "x" + Height + " P" + PlayerScore + " R" + RivalScore;
    }
}
