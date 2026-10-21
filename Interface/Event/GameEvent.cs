namespace FFF7R
{
    using System.Collections.Generic;

    /// <summary>
    /// 一次状态变更的通知。内核的 7 个 C# event 由适配层归一成这一条流。
    ///
    /// <para>
    /// <see cref="Sequence"/> 由内核在每次 Raise 前自增，**不是用来排序的**——
    /// 回调顺序本身就是触发顺序。它的用途是让表现层做丢包检测：
    /// 收到的序号不是 <c>lastSequence + 1</c>，说明中间漏了事件（中途订阅、帧内丢弃、
    /// 或某个订阅者抛异常截断了后续派发），此时应立即用 <see cref="IGameQuery"/>
    /// 拉一次全量快照 resync，而不是硬把当前事件套上去。
    /// </para>
    /// </summary>
    public abstract class GameEvent
    {
        public long Sequence { get; }

        protected GameEvent(long sequence)
        {
            Sequence = sequence;
        }

        public abstract GameEventKind Kind { get; }

        public override string ToString() => "#" + Sequence + " " + Kind;
    }

    /// <summary>棋子落到棋盘上（这是整条链路的起点）。</summary>
    public sealed class ChessPlacedEvent : GameEvent
    {
        public Int2 Pos { get; }
        public PlayerSeat Owner { get; }
        public string CardCode { get; }

        public ChessPlacedEvent(long sequence, Int2 pos, PlayerSeat owner, string cardCode)
            : base(sequence)
        {
            Pos = pos;
            Owner = owner;
            CardCode = cardCode;
        }

        public override GameEventKind Kind => GameEventKind.ChessPlaced;
    }

    /// <summary>棋子死亡并让出格子。</summary>
    public sealed class ChessDeadEvent : GameEvent
    {
        public Int2 Pos { get; }
        public PlayerSeat Owner { get; }
        public string CardCode { get; }
        public string CardName { get; }

        public ChessDeadEvent(long sequence, Int2 pos, PlayerSeat owner, string cardCode, string cardName)
            : base(sequence)
        {
            Pos = pos;
            Owner = owner;
            CardCode = cardCode;
            CardName = cardName;
        }

        public override GameEventKind Kind => GameEventKind.ChessDead;
    }

    /// <summary>某个既定格子的棋子吃到一次 buff（值可正可负）。</summary>
    public sealed class ChessBuffedEvent : GameEvent
    {
        public Int2 Pos { get; }
        public PlayerSeat Owner { get; }
        public int Value { get; }

        /// <summary>施加者的位置；无法确定时为 <see cref="Int2.Unknown"/>。</summary>
        public Int2 Source { get; }

        public ChessBuffedEvent(long sequence, Int2 pos, PlayerSeat owner, int value, Int2 source)
            : base(sequence)
        {
            Pos = pos;
            Owner = owner;
            Value = value;
            Source = source;
        }

        public override GameEventKind Kind => GameEventKind.ChessBuffed;
    }

    /// <summary>
    /// 来源棋子死亡，它施加出去的 buff 被一次性清理。
    /// <see cref="Cells"/> 只包含净变化不为零的格子。
    /// </summary>
    public sealed class BuffsExpiredEvent : GameEvent
    {
        public Int2 Source { get; }
        public IReadOnlyList<CellDelta> Cells { get; }

        public BuffsExpiredEvent(long sequence, Int2 source, IReadOnlyList<CellDelta> cells)
            : base(sequence)
        {
            Source = source;
            Cells = cells ?? new CellDelta[0];
        }

        public override GameEventKind Kind => GameEventKind.BuffsExpired;
    }

    /// <summary>一次落子导致的多格地形等级变化。<see cref="Pos"/> 是落子位置。</summary>
    public sealed class GridLevelsChangedEvent : GameEvent
    {
        public Int2 Pos { get; }
        public IReadOnlyList<CellDelta> Cells { get; }

        public GridLevelsChangedEvent(long sequence, Int2 pos, IReadOnlyList<CellDelta> cells)
            : base(sequence)
        {
            Pos = pos;
            Cells = cells ?? new CellDelta[0];
        }

        public override GameEventKind Kind => GameEventKind.GridLevelsChanged;
    }

    /// <summary>回合切换。</summary>
    public sealed class TurnEndedEvent : GameEvent
    {
        public PlayerSeat EndedPlayer { get; }
        public PlayerSeat NextPlayer { get; }
        public int Turns { get; }

        public TurnEndedEvent(long sequence, PlayerSeat endedPlayer, PlayerSeat nextPlayer, int turns)
            : base(sequence)
        {
            EndedPlayer = endedPlayer;
            NextPlayer = nextPlayer;
            Turns = turns;
        }

        public override GameEventKind Kind => GameEventKind.TurnEnded;
    }

    /// <summary>对局结束。<see cref="Winner"/> 为 <see cref="PlayerSeat.None"/> 表示平局。</summary>
    public sealed class GameEndedEvent : GameEvent
    {
        public PlayerSeat Winner { get; }
        public int Turns { get; }

        public GameEndedEvent(long sequence, PlayerSeat winner, int turns)
            : base(sequence)
        {
            Winner = winner;
            Turns = turns;
        }

        public override GameEventKind Kind => GameEventKind.GameEnded;
    }
}
