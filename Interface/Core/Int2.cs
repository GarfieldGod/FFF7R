namespace FFF7R
{
    using System;

    /// <summary>
    /// 棋盘坐标。约定：X 为列（横向），Y 为行（纵向），原点 (0,0) 在左上角。
    /// 契约层不复用内核的 Int2D，避免表现层被迫 using 内核命名空间。
    /// </summary>
    public readonly struct Int2 : IEquatable<Int2>
    {
        public int X { get; }
        public int Y { get; }

        public Int2(int x, int y)
        {
            X = x;
            Y = y;
        }

        /// <summary>“不存在的格子”／“来源未知”的哨兵值。</summary>
        public static readonly Int2 Unknown = new Int2(-1, -1);

        public bool IsUnknown => X < 0 || Y < 0;

        public bool Equals(Int2 other) => X == other.X && Y == other.Y;

        public override bool Equals(object obj) => obj is Int2 other && Equals(other);

        public override int GetHashCode() => (X * 397) ^ Y;

        public static bool operator ==(Int2 a, Int2 b) => a.Equals(b);

        public static bool operator !=(Int2 a, Int2 b) => !a.Equals(b);

        public override string ToString() => "(" + X + "," + Y + ")";
    }
}
