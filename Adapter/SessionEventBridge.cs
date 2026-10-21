namespace FFF7R.Adapter
{
    using System;
    using System.Collections.Generic;
    using FFF7RCore;

    /// <summary>
    /// 把内核 <see cref="EventSystem"/> 的 7 个 C# event 归一成一条 <see cref="GameEvent"/> 流。
    ///
    /// 这是表现层唯一不该看见内核事件的地方——其它任何类都不该直接 <c>+=</c> 内核事件。
    /// 事件按内核真实派发顺序同步转发，不做缓冲、不排序（回调顺序本身就是结算顺序）。
    /// </summary>
    internal sealed class SessionEventBridge : IDisposable
    {
        private readonly EventSystem events_;
        private readonly Action<GameEvent> sink_;

        private long lastSequence_;
        private int raisedCount_;
        private bool attached_;

        public long LastSequence => lastSequence_;

        /// <summary>自上次 <see cref="ResetCount"/> 以来转发出去的事件数。</summary>
        public int RaisedCount => raisedCount_;

        public SessionEventBridge(EventSystem events, Action<GameEvent> sink)
        {
            events_ = events;
            sink_ = sink;
            Attach();
        }

        public void ResetCount() => raisedCount_ = 0;

        //-------------------------------------------------------------------- 订阅

        private void Attach()
        {
            if (events_ == null || attached_) return;
            attached_ = true;

            events_.OnChessPlaced += HandlePlaced;
            events_.OnChessDead += HandleDead;
            events_.OnChessBuffed += HandleBuffed;
            events_.OnBuffsExpired += HandleBuffsExpired;
            events_.OnGridLevelsChanged += HandleGridLevelsChanged;
            events_.OnTurnEnd += HandleTurnEnd;
            events_.OnGameEnd += HandleGameEnd;
        }

        public void Dispose()
        {
            if (!attached_) return;
            attached_ = false;

            events_.OnChessPlaced -= HandlePlaced;
            events_.OnChessDead -= HandleDead;
            events_.OnChessBuffed -= HandleBuffed;
            events_.OnBuffsExpired -= HandleBuffsExpired;
            events_.OnGridLevelsChanged -= HandleGridLevelsChanged;
            events_.OnTurnEnd -= HandleTurnEnd;
            events_.OnGameEnd -= HandleGameEnd;
        }

        //-------------------------------------------------------------------- 转发

        private void Dispatch(GameEvent gameEvent)
        {
            lastSequence_ = gameEvent.Sequence;
            raisedCount_++;
            sink_?.Invoke(gameEvent);
        }

        private void HandlePlaced(object sender, EventSystem.ChessPlacedEventArgs e)
        {
            Dispatch(new ChessPlacedEvent(
                e.Sequence, e.Position.ToContract(), e.Owner.ToContract(), e.CardCode));
        }

        private void HandleDead(object sender, EventSystem.ChessDeadEventArgs e)
        {
            Dispatch(new ChessDeadEvent(
                e.Sequence,
                e.Position.ToContract(),
                e.Owner.ToContract(),
                e.Chess != null ? e.Chess.CardCode : null,
                e.Chess != null ? e.Chess.Name : null));
        }

        private void HandleBuffed(object sender, EventSystem.ChessBuffedEventArgs e)
        {
            Dispatch(new ChessBuffedEvent(
                e.Sequence, e.Position.ToContract(), e.Owner.ToContract(), e.Value, e.Source.ToContract()));
        }

        private void HandleBuffsExpired(object sender, EventSystem.BuffsExpiredEventArgs e)
        {
            List<CellDelta> cells = null;
            if (e.Entries != null)
            {
                cells = new List<CellDelta>(e.Entries.Count);
                for (int i = 0; i < e.Entries.Count; i++)
                {
                    EventSystem.BuffExpiredEntry entry = e.Entries[i];
                    cells.Add(new CellDelta(
                        entry.Position.ToContract(),
                        entry.Owner.ToContract(),
                        entry.Before,
                        entry.After));
                }
            }
            Dispatch(new BuffsExpiredEvent(e.Sequence, e.Source.ToContract(), cells));
        }

        private void HandleGridLevelsChanged(object sender, EventSystem.GridLevelsChangedEventArgs e)
        {
            List<CellDelta> cells = null;
            if (e.Changes != null)
            {
                cells = new List<CellDelta>(e.Changes.Count);
                for (int i = 0; i < e.Changes.Count; i++)
                {
                    EventSystem.GridLevelChange change = e.Changes[i];
                    // 地形等级变更没有“归属”概念，Owner 留给表现层按 After 自行判断
                    cells.Add(new CellDelta(
                        change.Position.ToContract(),
                        ((FFF7RCore.Effect.PosStatus)change.After).ToContract().Owner(),
                        change.Before,
                        change.After));
                }
            }
            Dispatch(new GridLevelsChangedEvent(e.Sequence, e.Input.pos.ToContract(), cells));
        }

        private void HandleTurnEnd(object sender, EventSystem.TurnEndedEventArgs e)
        {
            Dispatch(new TurnEndedEvent(
                e.Sequence, e.EndedPlayer.ToContract(), e.NextPlayer.ToContract(), e.Turns));
        }

        private void HandleGameEnd(object sender, EventSystem.GameEndedEventArgs e)
        {
            Dispatch(new GameEndedEvent(e.Sequence, e.Winner.ToContract(), e.Turns));
        }
    }
}
