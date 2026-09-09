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