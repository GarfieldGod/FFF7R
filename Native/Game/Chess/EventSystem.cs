namespace FFF7RCore {
    public class EventSystem
    {
        public event EventHandler<Input> OnChessPlaced;
        public event EventHandler<ChessDeadEventArgs> OnChessDead;
        public event EventHandler<ChessBuffedEventArgs> OnChessBuffed;
        public Action OnTurnEnd;
        public Action OnGameEnd;

        public void RaiseChessPlaced(Input input)
        {
            Log.Test($"OnChessPlaced", TextColor.YELLOW);
            Log.Test(" were Invoked");
            Log.TestLine($" ({input})", TextColor.BLACK);
            OnChessPlaced?.Invoke(this, input);
        }

        public void RaiseChessDead(ChessDeadEventArgs e)
        {
            Log.Test($"OnChessDead", TextColor.YELLOW);
            Log.Test(" were Invoked");
            Log.TestLine($" (Owner: {e.Owner} CardCode: {e.Chess.CardCode} Name: {e.Chess.Name})", TextColor.BLACK);
            OnChessDead?.Invoke(this, e);
        }

        public void RaiseChessBuffed(ChessBuffedEventArgs e)
        {
            Log.Test($"OnChessBuffed", TextColor.YELLOW);
            Log.Test(" were Invoked");
            Log.TestLine($" (Owner: {e.Owner} Pos: {e.Position} Value: {e.Value})", TextColor.BLACK);
            OnChessBuffed?.Invoke(this, e);
        }

        public void RaiseTurnEnd()
        {
            OnTurnEnd?.Invoke();
        }

        public void RaiseGameEnd()
        {
            OnGameEnd?.Invoke();
        }

        public void ClearAll()
        {
            OnChessPlaced = null;
            OnChessDead = null;
            OnChessBuffed = null;
            OnTurnEnd = null;
            OnGameEnd = null;
        }

        public class ChessDeadEventArgs : EventArgs
        {
            public ChessProperty Chess;
            public PlayerType Owner;
            public Int2D Pos;
            public ChessDeadEventArgs(ChessProperty chess, PlayerType owner, Int2D pos)
            {
                Chess = chess;
                Owner = owner;
                Pos = pos;
            }
        }

        public class ChessBuffedEventArgs : EventArgs
        {
            public int Value;
            public PlayerType Owner;
            public Int2D Position;
            public ChessBuffedEventArgs(int value, PlayerType owner, Int2D position)
            {
                Value = value;
                Owner = owner;
                Position = position;
            }
        }
    }
}