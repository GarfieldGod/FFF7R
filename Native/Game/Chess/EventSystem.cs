namespace FFF7RCore {
    using System;
    using System.Collections.Generic;

    public class EventSystem
    {
        // 事件序号：每次 Raise 之前自增，随事件载荷一起下发。
        // 适配层按 Sequence 排序即可还原真实结算顺序（同一帧内的嵌套触发也能区分）。
        private long sequence_;
        public long Sequence => sequence_;

        public event EventHandler<ChessPlacedEventArgs> OnChessPlaced;
        public event EventHandler<ChessDeadEventArgs> OnChessDead;
        public event EventHandler<ChessBuffedEventArgs> OnChessBuffed;
        public event EventHandler<BuffsExpiredEventArgs> OnBuffsExpired;
        public event EventHandler<GridLevelsChangedEventArgs> OnGridLevelsChanged;
        public event EventHandler<TurnEndedEventArgs> OnTurnEnd;
        public event EventHandler<GameEndedEventArgs> OnGameEnd;

        public void RaiseChessPlaced(Input input)
        {
            sequence_++;
            Log.Test($"OnChessPlaced", TextColor.YELLOW);
            Log.Test(" were Invoked");
            Log.TestLine($" ({input})", TextColor.BLACK);
            OnChessPlaced?.Invoke(this, new ChessPlacedEventArgs(input.pos, input.playerType, input.chess?.CardCode, sequence_));
        }

        public void RaiseChessDead(ChessDeadEventArgs e)
        {
            sequence_++;
            e.Sequence = sequence_;
            Log.Test($"OnChessDead", TextColor.YELLOW);
            Log.Test(" were Invoked");
            Log.TestLine($" (Owner: {e.Owner} Pos: {e.Position} CardCode: {e.Chess.CardCode} Name: {e.Chess.Name})", TextColor.BLACK);
            OnChessDead?.Invoke(this, e);
        }

        public void RaiseChessBuffed(ChessBuffedEventArgs e)
        {
            sequence_++;
            e.Sequence = sequence_;
            Log.Test($"OnChessBuffed", TextColor.YELLOW);
            Log.Test(" were Invoked");
            Log.TestLine($" (Owner: {e.Owner} Pos: {e.Position} Source: {e.Source} Value: {e.Value})", TextColor.BLACK);
            OnChessBuffed?.Invoke(this, e);
        }

        public void RaiseBuffsExpired(BuffsExpiredEventArgs e)
        {
            sequence_++;
            e.Sequence = sequence_;
            Log.Test($"OnBuffsExpired", TextColor.YELLOW);
            Log.Test(" were Invoked");
            Log.TestLine($" (Source: {e.Source} Affected: {e.Entries.Count})", TextColor.BLACK);
            OnBuffsExpired?.Invoke(this, e);
        }

        public void RaiseGridLevelsChanged(GridLevelsChangedEventArgs e)
        {
            sequence_++;
            e.Sequence = sequence_;
            Log.Test($"OnGridLevelsChanged", TextColor.YELLOW);
            Log.Test(" were Invoked");
            Log.TestLine($" (Changed: {e.Changes.Count})", TextColor.BLACK);
            OnGridLevelsChanged?.Invoke(this, e);
        }

        public void RaiseTurnEnd(TurnEndedEventArgs e)
        {
            sequence_++;
            e.Sequence = sequence_;
            OnTurnEnd?.Invoke(this, e);
        }

        public void RaiseGameEnd(GameEndedEventArgs e)
        {
            sequence_++;
            e.Sequence = sequence_;
            OnGameEnd?.Invoke(this, e);
        }

        public void ClearAll()
        {
            OnChessPlaced = null;
            OnChessDead = null;
            OnChessBuffed = null;
            OnBuffsExpired = null;
            OnGridLevelsChanged = null;
            OnTurnEnd = null;
            OnGameEnd = null;
        }

        public class ChessPlacedEventArgs : EventArgs
        {
            public Int2D Position;
            public PlayerType Owner;
            public string CardCode;
            public long Sequence;
            public ChessPlacedEventArgs(Int2D position, PlayerType owner, string cardCode, long sequence)
            {
                Position = position;
                Owner = owner;
                CardCode = cardCode;
                Sequence = sequence;
            }
        }

        public class ChessDeadEventArgs : EventArgs
        {
            public ChessProperty Chess;
            public PlayerType Owner;
            public Int2D Position;   // 死亡棋子所在格
            public long Sequence;
            public ChessDeadEventArgs(ChessProperty chess, PlayerType owner, Int2D position)
            {
                Chess = chess;
                Owner = owner;
                Position = position;
            }
        }

        public class ChessBuffedEventArgs : EventArgs
        {
            public int Value;
            public PlayerType Owner;
            public Int2D Position;
            public Int2D Source;     // buff 施加者位置；无法确定时为 Int2D.Unknown
            public long Sequence;
            public ChessBuffedEventArgs(int value, PlayerType owner, Int2D position, Int2D source)
            {
                Value = value;
                Owner = owner;
                Position = position;
                Source = source;
            }
        }

        // 来源棋子死亡，导致它施加出去的 buff 被一次性清理
        public class BuffsExpiredEventArgs : EventArgs
        {
            public Int2D Source;                    // 被清理的来源位置（棋子死亡处）
            public List<BuffExpiredEntry> Entries;   // 受影响格子及其 Level 前后值
            public long Sequence;
            public BuffsExpiredEventArgs(Int2D source, List<BuffExpiredEntry> entries)
            {
                Source = source;
                Entries = entries;
            }
        }

        public struct BuffExpiredEntry
        {
            public Int2D Position;
            // 该格归属；空格/无主格为 PlayerType.Null（不能一律当作 RIVAL）。
            public PlayerType Owner;
            public int Before;   // 清理前的 Level（含全部 buff）
            public int After;    // 清理后的 Level
            public BuffExpiredEntry(Int2D position, PlayerType owner, int before, int after)
            {
                Position = position;
                Owner = owner;
                Before = before;
                After = after;
            }
        }

        // 格子等级（PosStatus）变化，一次落子可能同时影响多格
        public class GridLevelsChangedEventArgs : EventArgs
        {
            public Input Input;
            public List<GridLevelChange> Changes;
            public long Sequence;
            public GridLevelsChangedEventArgs(Input input, List<GridLevelChange> changes)
            {
                Input = input;
                Changes = changes;
            }
        }

        public struct GridLevelChange
        {
            public Int2D Position;
            public int Before;   // (int)PosStatus
            public int After;    // (int)PosStatus
            public GridLevelChange(Int2D position, int before, int after)
            {
                Position = position;
                Before = before;
                After = after;
            }
        }

        public class TurnEndedEventArgs : EventArgs
        {
            public PlayerType EndedPlayer;   // 刚结束回合的一方
            public PlayerType NextPlayer;    // 接手回合的一方
            public int Turns;                // 已完成的回合数（NextTurn 之后的值）
            public long Sequence;
            public TurnEndedEventArgs(PlayerType endedPlayer, PlayerType nextPlayer, int turns)
            {
                EndedPlayer = endedPlayer;
                NextPlayer = nextPlayer;
                Turns = turns;
            }
        }

        public class GameEndedEventArgs : EventArgs
        {
            public PlayerType Winner;    // PlayerType.Null 表示平局
            public int Turns;
            public long Sequence;
            public GameEndedEventArgs(PlayerType winner, int turns)
            {
                Winner = winner;
                Turns = turns;
            }
        }
    }
}
