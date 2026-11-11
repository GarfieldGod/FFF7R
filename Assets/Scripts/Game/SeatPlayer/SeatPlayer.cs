using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FFF7R;

public sealed class SeatPlayer
{
    public PlayerSeat Seat { get; }
    public SeatPlayer(PlayerSeat seat)
    {
        Seat = seat;
    }

    public void OnTurnStart(ISeatActionSource src) => src.OnTurnStart(Seat);

    public void Tick(IGameQuery q, ISeatCommands cmd, ISeatActionSource src)
    {
        GameStateView st = q.State;
        if (!st.IsGaming || st.Current != Seat) return;

        if (!src.TryTake(q, Seat, out SeatAction a)) return;

        CommandResult r;
        switch (a.ActionKind)
        {
            case SeatAction.Kind.Place: r = cmd.Place(Seat, a.HandIndex, a.Pos); break;
            case SeatAction.Kind.Pass:  r = cmd.Pass(Seat); break;
            default: return;
        }

        src.OnResult(r);
    }

    public void OnTurnEnd(ISeatActionSource src) => src.OnTurnEnd(Seat);
}
