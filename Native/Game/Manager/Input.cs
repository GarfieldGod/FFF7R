namespace FFF7RCore {
    public struct Input {
        public Int2D pos;
        public ChessProperty chess;
        public PlayerType playerType;
        public Input(Int2D pos, ChessProperty chess, PlayerType playerType) {
            this.pos = pos;
            this.chess = chess;
            this.playerType = playerType;
        }
        public Input(Int2D pos, string cardCode, PlayerType playerType) {
            this.pos = pos;
            this.chess = Property.GetChessProperty(cardCode);
            this.playerType = playerType;
        }
        public readonly bool Empty => chess == null;
    }
}