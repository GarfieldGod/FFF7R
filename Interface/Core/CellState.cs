namespace FFF7R
{
    /// <summary>
    /// 单格地形状态。数值与内核 PosStatus 的整数值保持一致，
    /// 适配层可以直接强转，但表现层不应依赖具体数值——请按枚举分支。
    /// </summary>
    public enum CellState
    {
        LevelOnePlayer = 1,
        LevelTwoPlayer = 2,
        LevelThreePlayer = 3,
        Empty = 10,
        LevelOneRival = 11,
        LevelTwoRival = 12,
        LevelThreeRival = 13,
        OccupiedPlayer = 14,
        OccupiedRival = 15,
    }

    public static class CellStateExtensions
    {
        /// <summary>格子上是否站着棋子（含被夺取后的占据态）。</summary>
        public static bool IsOccupied(this CellState state)
            => state == CellState.OccupiedPlayer || state == CellState.OccupiedRival;

        /// <summary>格子归属；未占领的等级格归属由数值区间决定，空格返回 <see cref="PlayerSeat.None"/>。</summary>
        public static PlayerSeat Owner(this CellState state)
        {
            switch (state)
            {
                case CellState.LevelOnePlayer:
                case CellState.LevelTwoPlayer:
                case CellState.LevelThreePlayer:
                case CellState.OccupiedPlayer:
                    return PlayerSeat.Player;
                case CellState.LevelOneRival:
                case CellState.LevelTwoRival:
                case CellState.LevelThreeRival:
                case CellState.OccupiedRival:
                    return PlayerSeat.Rival;
                default:
                    return PlayerSeat.None;
            }
        }

        public static int ToLevel(this CellState state)
        {
            switch (state)
            {
                case CellState.LevelOnePlayer:
                case CellState.LevelOneRival:
                    return 1;
                case CellState.LevelTwoPlayer:
                case CellState.LevelTwoRival:
                    return 2;
                case CellState.LevelThreePlayer:
                case CellState.LevelThreeRival:
                    return 3;
                case CellState.OccupiedPlayer:
                case CellState.OccupiedRival:
                case CellState.Empty:
                    return 0;
                default:
                    return 0;
            }
        }
    }
}
