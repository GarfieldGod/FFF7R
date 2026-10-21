namespace FFF7R.Adapter
{
    using System.Collections.Generic;
    using FFF7RCore;
    using FFF7RCore.Effect;

    /// <summary>
    /// 内核类型 ↔ 契约类型的唯一转换点。
    ///
    /// 全工程只有这个文件允许同时看见 <c>FFF7RCore</c> 和 <c>FFF7R</c> 两套类型；
    /// 其它适配器只调用这里的转换方法，不要在别处再写一遍映射。
    /// </summary>
    internal static class KernelMapping
    {
        //-------------------------------------------------------------------- 坐标

        public static Int2 ToContract(this Int2D v) => new Int2(v.x, v.y);

        public static Int2D ToKernel(this Int2 v) => new Int2D(v.X, v.Y);

        //-------------------------------------------------------------------- 席位 / 阶段

        public static PlayerSeat ToContract(this PlayerType v)
        {
            switch (v)
            {
                case PlayerType.PLAYER: return PlayerSeat.Player;
                case PlayerType.RIVAL: return PlayerSeat.Rival;
                default: return PlayerSeat.None;
            }
        }

        public static PlayerType ToKernel(this PlayerSeat v)
        {
            switch (v)
            {
                case PlayerSeat.Player: return PlayerType.PLAYER;
                case PlayerSeat.Rival: return PlayerType.RIVAL;
                default: return PlayerType.Null;
            }
        }

        public static MatchStatus ToContract(this GameStatus v)
        {
            switch (v)
            {
                case GameStatus.INIT: return MatchStatus.Init;
                case GameStatus.DISPENSE_CHESS: return MatchStatus.Dispense;
                case GameStatus.REDISPENSE_CHESS: return MatchStatus.ReDispense;
                case GameStatus.Ready: return MatchStatus.Ready;
                case GameStatus.GAMING: return MatchStatus.Gaming;
                // GAME_END / COMPUTE_RESULT / GAME_OVER 是结算的内部步骤，对外合并成一个
                default: return MatchStatus.GameOver;
            }
        }

        public static CellState ToContract(this PosStatus v) => (CellState)(int)v;

        //-------------------------------------------------------------------- 卡牌定义

        /// <summary>
        /// 内核的局部矩阵是 <c>[y][x]</c>（外层是行），契约层统一成 <c>[x][y]</c>（外层是列）。
        /// 转置只在这里发生一次。
        /// </summary>
        public static GridPattern ToPattern(List<List<int>> source)
        {
            if (source == null || source.Count == 0 || source[0] == null) return null;

            int rows = source.Count;      // y
            int cols = source[0].Count;   // x
            int[][] columns = new int[cols][];

            for (int x = 0; x < cols; x++)
            {
                int[] column = new int[rows];
                for (int y = 0; y < rows; y++)
                {
                    List<int> row = source[y];
                    column[y] = row != null && x < row.Count ? row[x] : 0;
                }
                columns[x] = column;
            }
            return new GridPattern(cols, rows, columns);
        }

        public static CardDefView ToCardDef(ChessProperty property)
        {
            if (property == null) return null;

            return new CardDefView(
                property.CardCode,
                property.Name,
                property.Level,
                property.Cost,
                property.Description,
                ToPattern(property.PosEffect),
                ToPattern(property.CardEffect?.Scope),
                SummarizeEffect(property.CardEffect));
        }

        private static string SummarizeEffect(CardEffectEntry effect)
        {
            if (effect == null) return null;

            string summary = effect.Condition + " / " + effect.Operation;
            if (effect.Value != 0) summary += " " + effect.Value;
            if (effect.CardCodeList != null && effect.CardCodeList.Count > 0)
            {
                summary += " [" + string.Join(",", effect.CardCodeList) + "]";
            }
            return summary;
        }

        //-------------------------------------------------------------------- 棋盘快照

        /// <summary>
        /// 注意：<c>ChessPad.Width/Height</c> 这两个属性的名字和实际含义是反的
        /// （Width 返回的是 y 方向长度），这里一律按 <c>Size.x / Size.y</c> 取，不要用它们。
        /// </summary>
        public static BoardSnapshot ToBoard(ChessPad pad)
        {
            if (pad == null) return null;

            int width = pad.Size.x;
            int height = pad.Size.y;
            CellView[] cells = new CellView[width * height];
            int playerScore = 0;
            int rivalScore = 0;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    PadGrid grid = pad[x, y];
                    ChessProperty property = grid.Chess;
                    CellState state = grid.Status.ToContract();

                    cells[x * height + y] = new CellView(
                        new Int2(x, y),
                        state,
                        property != null ? property.CardCode : null,
                        grid.Level,                                // 含 buff
                        property != null ? property.Level : 0);    // 基础战力

                    if (state == CellState.OccupiedPlayer) playerScore += grid.Level;
                    else if (state == CellState.OccupiedRival) rivalScore += grid.Level;
                }
            }

            return new BoardSnapshot(width, height, cells, playerScore, rivalScore);
        }
    }
}
