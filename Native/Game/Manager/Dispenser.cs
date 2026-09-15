namespace FFF7RCore {
    using System;
    using System.Collections.Generic;

    public class Dispenser
    {
        public List<string> ChessPool => chessPool_;
        public int Count => chessPool_.Count;
        private List<string> chessPool_;

        public Dispenser(List<string> chessPool){
            chessPool_ = chessPool;
        }

        public string this[int index]
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

        public string Dispense() {
            if (chessPool_.Count == 0) {
                return null;
            }
            System.Random random = new System.Random();
            int randomIndex = random.Next(chessPool_.Count);
            string chess = chessPool_[randomIndex];
            chessPool_.RemoveAt(randomIndex);
            return chess;
        }
    }
}