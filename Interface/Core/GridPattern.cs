namespace FFF7R
{
    using System.Collections.Generic;

    /// <summary>
    /// 卡牌的局部坐标系矩阵（放置影响范围、效果范围）。
    ///
    /// 索引约定与 <see cref="Int2"/> 统一：<c>this[x, y]</c>，X 为列、Y 为行。
    /// 内层存储按列存放（columns[x][y]），因此从内核的 <c>[y][x]</c> 转换时必须转置，
    /// 这一步由适配层负责——契约层对外只暴露一种口径，避免“两处索引方向相反”的老问题。
    /// </summary>
    public sealed class GridPattern
    {
        private readonly int[][] columns_;

        public int Width { get; }
        public int Height { get; }

        /// <summary>矩阵中心（落子点所在的格子）。宽高为奇数时两侧对称。</summary>
        public int CenterX => Width / 2;
        public int CenterY => Height / 2;

        public static readonly GridPattern Empty = new GridPattern(0, 0, new int[0][]);

        public GridPattern(int width, int height, int[][] columns)
        {
            Width = width;
            Height = height;
            columns_ = columns;
        }

        public bool IsInPattern(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;

        public int this[int x, int y]
        {
            get
            {
                if (!IsInPattern(x, y)) return 0;
                int[] column = columns_[x];
                return column == null || y >= column.Length ? 0 : column[y];
            }
        }

        /// <summary>遍历所有非零格，坐标相对落子点（已减去中心）。</summary>
        public IEnumerable<Int2> NonZeroCells()
        {
            for (int x = 0; x < Width; x++)
            {
                int[] column = columns_[x];
                if (column == null) continue;
                for (int y = 0; y < column.Length; y++)
                {
                    if (column[y] != 0) yield return new Int2(x, y);
                }
            }
        }
    }
}
