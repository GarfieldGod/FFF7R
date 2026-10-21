namespace FFF7R
{
    using System.Collections.Generic;

    /// <summary>
    /// 开一局所需的全部参数。表现层只填这个，不接触 ChessPad / GameConfig。
    /// </summary>
    public sealed class GameSetup
    {
        public IReadOnlyList<string> PlayerPool { get; }
        public IReadOnlyList<string> RivalPool { get; }

        public int BoardWidth { get; }
        public int BoardHeight { get; }

        /// <summary>true 时按内核的标准初始地形铺盘，忽略自定义格子内容。</summary>
        public bool StandardBoard { get; }

        public PlayerSeat FirstSeat { get; }

        /// <summary>回合上限；0 表示不限制。</summary>
        public int TurnLimit { get; }

        /// <summary>
        /// 牌序种子；0 表示随机（实际值可从 <see cref="IGameSession.Seed"/> 读回）。
        /// 同一个种子 + 同一串指令 = 完全相同的对局。
        /// </summary>
        public int Seed { get; }

        public GameSetup(
            IReadOnlyList<string> playerPool,
            IReadOnlyList<string> rivalPool,
            int boardWidth = 3,
            int boardHeight = 5,
            bool standardBoard = true,
            PlayerSeat firstSeat = PlayerSeat.Player,
            int turnLimit = 0,
            int seed = 0)
        {
            PlayerPool = playerPool ?? new string[0];
            RivalPool = rivalPool ?? new string[0];
            BoardWidth = boardWidth;
            BoardHeight = boardHeight;
            StandardBoard = standardBoard;
            FirstSeat = firstSeat;
            TurnLimit = turnLimit;
            Seed = seed;
        }
    }
}
