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
            Log.TestLine("OnChessPlaced were Invoked");
            OnChessPlaced?.Invoke(this, input);
        }

        public void RaiseChessDead(ChessDeadEventArgs e)
        {
            Log.TestLine("OnChessDead were Invoked");
            OnChessDead?.Invoke(this, e);
        }

        public void RaiseChessBuffed(ChessBuffedEventArgs e)
        {
            Log.TestLine("OnChessBuffed were Invoked");
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
            public ChessDeadEventArgs(ChessProperty chess, PlayerType owner)
            {
                Chess = chess;
                Owner = owner;
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