namespace FFF7R
{
    /// <summary>
    /// 单格的数值变化。用于「地形等级变更」和「buff 到期清理」两类事件，
    /// 表现层只需按 <see cref="Before"/> → <see cref="After"/> 播放数值滚动/高亮。
    /// </summary>
    public readonly struct CellDelta
    {
        public Int2 Pos { get; }
        public PlayerSeat Owner { get; }
        public int Before { get; }
        public int After { get; }

        public CellDelta(Int2 pos, PlayerSeat owner, int before, int after)
        {
            Pos = pos;
            Owner = owner;
            Before = before;
            After = after;
        }

        public int Delta => After - Before;

        public override string ToString()
            => Pos + " " + Before + "→" + After + " (" + (Delta >= 0 ? "+" : "") + Delta + ")";
    }
}
