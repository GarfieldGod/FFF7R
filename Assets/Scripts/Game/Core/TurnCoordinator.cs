using FFF7R;
using System;

public class TurnCoordinator
{
    readonly GameRuntime _runtime;
    readonly ISeatActionSource[] _sources;
    readonly SeatPlayer[] _seats = new SeatPlayer[3];

    int _lastTurn = -1;
    bool _ended = false;

    public event Action<PlayerSeat> TurnBegan;
    public event Action<PlayerSeat> TurnEnd;
    public event Action<IGameQuery> MatchEnded;

    public TurnCoordinator(GameRuntime runtime, ISeatActionSource[] sources)
    {
        _runtime = runtime;
        _sources = sources;
        _seats[(int)PlayerSeat.Player] = new SeatPlayer(PlayerSeat.Player);
        _seats[(int)PlayerSeat.Rival]  = new SeatPlayer(PlayerSeat.Rival);
    }

    public void Tick()
    {
        if (_runtime == null || _runtime.IsBusy) return;
        if (_runtime.Query == null) return;

        if (_runtime.Anim == null || _runtime.Anim.IsBusy) return;

        var st = _runtime.Query.State;

        if (st.IsOver)
        {
            if (!_ended) { _ended = true; MatchEnded?.Invoke(_runtime.Query); }
            return;
        }
        if (!st.IsGaming) return;

        var seat = _seats[(int)st.Current];
        var actionSrc = _sources[(int)st.Current];

        if (st.Turns != _lastTurn)
        {
            _lastTurn = st.Turns;
            TurnBegan?.Invoke(st.Current);
            seat.OnTurnStart(actionSrc);
        }

        seat.Tick(_runtime.Query, _runtime, actionSrc);

        var newSt = _runtime.Query.State;
        if (st.Turns != newSt.Turns)
        {
            TurnEnd?.Invoke(st.Current);
            seat.OnTurnEnd(actionSrc);
        }
    }
}
