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

        /// <summary>
        /// 内核的拒绝原因 → 契约的拒绝原因。两边是各自独立的枚举，不要互相强转：
        /// 内核枚举描述的是"规则为什么不过"，契约枚举描述的是"给玩家看什么提示"。
        /// </summary>
        public static RejectReason ToContract(this InputReject v)
        {
            switch (v)
            {
                case InputReject.None: return RejectReason.None;
                case InputReject.NotGaming: return RejectReason.NotGaming;
                case InputReject.NotYourTurn: return RejectReason.NotYourTurn;
                case InputReject.EmptyInput: return RejectReason.CardNotFound;
                case InputReject.CardNotInHand: return RejectReason.CardNotInHand;
                case InputReject.OutOfBoard: return RejectReason.OutOfBoard;
                case InputReject.CellOccupied: return RejectReason.CellOccupied;
                case InputReject.CellLevelTooLow: return RejectReason.CellLevelTooLow;
                case InputReject.NoOwnChessToCover: return RejectReason.NoOwnChessToCover;
                default: return RejectReason.IllegalPlacement;
            }
        }

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
        /// 内核棋盘 → 契约快照。
        /// <para>
        /// 本工程的坐标约定：<b>x 是高度方向（纵向），y 是宽度方向（横向）</b>。
        /// 所以 <c>Width = pad.Size.y</c>、<c>Height = pad.Size.x</c> —— 名字取自屏幕朝向，
        /// 不是 <c>Size</c> 的分量名，两者交叉是刻意的。
        /// </para>
        /// 存放顺序与 <see cref="BoardSnapshot"/> 的索引器一致：<c>cells[x * Width + y]</c>。
        /// </summary>
        public static BoardSnapshot ToBoard(ChessPad pad)
        {
            if (pad == null) return null;

            int w = pad.Size.y;   // 宽度方向的格数（y 轴）
            int h = pad.Size.x;   // 高度方向的格数（x 轴）
            CellView[] cells = new CellView[w * h];

            List<int> playerScoreList = new List<int>(h);
            List<int> rivalScoreList = new List<int>(h);

            for (int x = 0; x < h; x++)
            {
                int ps = 0;
                int rs = 0;
                for (int y = 0; y < w; y++)
                {
                    PadGrid grid = pad[x, y];
                    ChessProperty property = grid.Chess;
                    CellState state = grid.Status.ToContract();

                    cells[x * w + y] = new CellView(
                        new Int2(x, y),
                        state,
                        property != null ? property.CardCode : null,
                        grid.Level,                                // 含 buff
                        property != null ? property.Level : 0);    // 基础战力

                    if (state == CellState.OccupiedPlayer) ps += grid.Level;
                    else if (state == CellState.OccupiedRival) rs += grid.Level;
                }
                playerScoreList.Add(ps);
                rivalScoreList.Add(rs);
            }

            return new BoardSnapshot(w, h, cells, playerScoreList, rivalScoreList);
        }
    }
}
