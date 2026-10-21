namespace FFF7RCore {
    using System;
    using System.Collections.Generic;

    public class Dispenser
    {
        public List<Chess> ChessPool => chessPool_;
        public int Count => chessPool_.Count;
        private List<Chess> chessPool_;

        /// <summary>
        /// 发牌用的随机源。由 <see cref="Game"/> 按种子派生后注入，保证同种子同牌序；
        /// 传 null 时退化为无种子随机（老行为）。
        /// 注意：这里持有实例而不是每次 <see cref="Dispense"/> 里 new，否则无法复现。
        /// </summary>
        private readonly Random random_;

        public Dispenser(List<Chess> chessPool) : this(chessPool, null) { }

        public Dispenser(List<Chess> chessPool, Random random){
            chessPool_ = chessPool;
            random_ = random ?? new Random();
        }

        public Chess this[int index]
        {
            get
            {
                if (index < 0 || index >= chessPool_.Count)
                {
                    return null;
                }
                return chessPool_[index];
            }
            set
            {
                chessPool_[index] = value;
            }
        }

        public Dispenser(List<string> chessName) : this(chessName, null) { }

        public Dispenser(List<string> chessName, Random random){
            List<Chess> chessPool = new List<Chess>{};
            foreach(string name in chessName) {
                ChessProperty chessProperty = Property.GetChessProperty(name);
                Chess chess = new Chess(chessProperty);
                chessPool.Add(chess);
            }
            chessPool_ = chessPool;
            random_ = random ?? new Random();
        }

        public Chess Dispense() {
            if (chessPool_.Count == 0) {
                return null;
            }
            int randomIndex = random_.Next(chessPool_.Count);
            Chess chess = chessPool_[randomIndex];
            chessPool_.RemoveAt(randomIndex);
            return chess;
        }
    }
}