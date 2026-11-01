namespace FFF7RCore {
    using System.Collections.Generic;

    public class GamePlayer {
        public PlayerType Type => playerType;
        private readonly Dispenser dispenser;
        private readonly Selector selector;
        private readonly PlayerType playerType;

        public GamePlayer(Dispenser dispenser, Selector selector, PlayerType playerType) {
            this.dispenser = dispenser;
            this.selector = selector;
            this.playerType = playerType;
        }

        public Chess this[int index]
        {
            get => selector[index];
        }

        /// <summary>
        /// 这一方还有没有牌可出。手牌为空即无法行动（补牌是即时的，手牌空 ⟺ 牌池也空）。
        /// <c>Game.HasNextTurn</c> 用它判定"双方都动不了 → 对局结束"。
        /// </summary>
        public bool CanInput => selector.Count > 0;

        /// <summary>牌池里还剩多少张（不含手牌）。</summary>
        public int PoolCount => dispenser.Count;

        public void InitDespense(int initNum) {
            while(initNum > 0) {
                GetCardFromCardPool();
                initNum--;
            }
        }

        /// <summary>
        /// 丢弃指定手牌并立刻补一张。返回是否真的移除成功。
        /// 出牌消耗必须走这里，否则同一张牌可以无限次打出。
        /// </summary>
        public bool DiscardFromHand(int index) {
            if (!selector.RemoveAt(index)) return false;
            GetCardFromCardPool();   // 牌池见底时补不进来，手牌自然变少
            return true;
        }

        public void ReDispense(List<int> index) {
            index.Sort();

            string[] cardsToReturn = new string[index.Count];
            for (int i = index.Count - 1; i >= 0; i--) {
                cardsToReturn[i] = selector[index[i]].Property.CardCode;
                selector.RemoveAt(index[i]);
            }
            foreach (int i in index) {
                selector.Add(dispenser.Dispense());
            }
            dispenser.Add(cardsToReturn);
        }

        /// <summary>从牌池抽一张进手牌；牌池空时什么都不做。</summary>
        public void GetCardFromCardPool() {
            selector.Add(dispenser.Dispense());
        }

        public List<Chess> ChessInHand
        {
            get
            {
                return selector.ChessPool;
            }
            set
            {
                selector.ChessPool = value;
            }
        }

        // public ChessPad GetChessPad() {
        //     return inputer.GetChessPad();
        // }

        // public int Select(Chess chess) {
        //     int index = selector.GetIndex(chess);
        //     if (index != -1) {
        //         selector.Preview(index);
        //     }
        //     return index;
        // }

        // public Chess Select(int index) {
        //     Chess selectedChess = selector.GetChess(index);
        //     if (selectedChess != null) {
        //         selector.Preview(index);
        //     }
        //     return selectedChess;
        // }

        // public void RestoreSelect() {
        //     selector.CancelPreview();
        // }

        // public bool AddInput(Input input) {
        //     return inputer.AddInput(input);
        // }

        // public void RestoreInput() {
        //     RestoreSelect();
        //     inputer.RestoreInput();
        // }

        // public void CommitInput() {
        //     inputer.CommitInput();
        //     selector.Commit();
        //     GetCardFromCardPool();
        // }
    }
}