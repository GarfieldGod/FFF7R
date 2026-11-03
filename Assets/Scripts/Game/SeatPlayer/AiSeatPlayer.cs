using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FFF7R;
using FFF7R.Adapter;

public sealed class AiSeatPlayer: ISeatPlayer
{
    public PlayerSeat Seat { get; }
    // readonly GameRuntime gameRuntime;
    public IAiBrain AiBrain { get; set; }
    public AiSeatPlayer(PlayerSeat seat)
    {
        Seat = seat;
        AiBrain = new AiBrain();
    }

    int _lastTurn = -1;
    public void Tick(IGameQuery query, ISeatCommands commands, ISeatInputSource input)
    {
        var st = query.State;
        if (st.Turns == _lastTurn) return;
        _lastTurn = st.Turns;
        AiTurn(query, commands);
    }

    void AiTurn(IGameQuery q, ISeatCommands c){
        bool ret = AiBrain.TryDecide(q, Seat, out AiDecision decision);

        if (ret) {
            c.Place(Seat, decision.HandIndex, decision.Pos);
            Debug.Log($"AI {Seat} placed {decision.HandIndex} at {decision.Pos}");
        }
        else
        {
            c.Pass(Seat);
            Debug.Log($"AI {Seat} passed");
        }
    }
}