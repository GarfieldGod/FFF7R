namespace FFF7RCore {
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

        public string this[int index]
        {
            get => selector[index];
        }

        public void InitDespense(int initNum) {
            while(initNum > 0) {
                GetCardFromCardPool();
                initNum--;
            }
        }

        public void ReDispense(List<int> index) {
            
        }

        public void GetCardFromCardPool() {
            selector.Add(dispenser.Dispense());
        }

        public List<string> ChessInHand
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
    }
}