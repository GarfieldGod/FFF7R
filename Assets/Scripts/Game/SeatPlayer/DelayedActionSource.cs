using System;
using FFF7R;
using UnityEngine;

public sealed class DelayedActionSource : ISeatActionSource
{
    readonly ISeatActionSource _inner;
    readonly float _delay;

    int _currentTurn = -1;
    float _startAt;

    public DelayedActionSource(ISeatActionSource inner, float delay)
    => (_inner, _delay) = (inner, delay);
    public void OnTurnStart(PlayerSeat seat) => _inner.OnTurnStart(seat);
    public void OnTurnEnd(PlayerSeat seat) => _inner.OnTurnEnd(seat);
    public bool TryTake(IGameQuery q, PlayerSeat seat, out SeatAction action)
    {
        int turn = q.State.Turns;
        if (_currentTurn != turn) { _currentTurn = turn; _startAt = Time.time; }
        if (Time.time - _startAt < _delay) { action = default; return false; }
        return _inner.TryTake(q, seat, out action);
    }

    public void OnResult(CommandResult r) => _inner.OnResult(r);
}