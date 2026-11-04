using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FFF7R;
using FFF7R.Adapter;

public sealed class AiActionSource: ISeatActionSource
{
    public IAiBrain AiBrain { get; set; }
    public AiActionSource()
    {
        AiBrain = new AiBrain();
    }

    int _decidedTurn = -1;
    int _failedTurn  = -1;
    SeatAction _cached;

    public void OnResult(CommandResult r) { if (!r.Ok) _failedTurn = _decidedTurn; }

    public bool TryTake(IGameQuery query, PlayerSeat seat, out SeatAction action)
    {
        int turn = query.State.Turns;

        if (_failedTurn == turn) { action = SeatAction.Pass(); return true; }
        if (_decidedTurn == turn) { action = _cached; return true; }

        _decidedTurn = turn;
        _cached = AiBrain.TryDecide(query, seat, out var d)
            ? SeatAction.Place(d.HandIndex, d.Pos)
            : SeatAction.Pass();
        action = _cached;
        return true;
    }
}