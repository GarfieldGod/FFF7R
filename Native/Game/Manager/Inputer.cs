using System.Collections.Generic;

public enum InputerType {
    PLAYER,
    RIVAL,
    AI
}

public struct Input {
    public Int2D pos;
    public Chess chess;
    public Input(Int2D pos, Chess chess) {
        this.pos = pos;
        this.chess = chess;
    }
    public bool Empty() {
        if(chess == null) {
            return true;
        }
        return false;
    }
}

public class Inputer {
    private Dispenser dispenser_;
    private Selector selector_;
    private ChessPad chessPad_;
    private InputerType inputerType_;
    public Inputer(Dispenser dispenser, Selector selector, ChessPad chessPad, InputerType inputerType = InputerType.PLAYER) {
        dispenser_ = dispenser;
        selector_ = selector;
        chessPad_ = chessPad;
        inputerType_ = inputerType;
    }
}