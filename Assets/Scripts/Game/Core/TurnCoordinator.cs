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

        var st = _runtime.Query.State;

        if (st.IsOver)
        {
            if (!_ended) { _ended = true; MatchEnded?.Invoke(_runtime.Query); }
            return;
        }
        if (!st.IsGaming) return;

        if (st.Turns != _lastTurn)
        {
            _lastTurn = st.Turns;
            TurnBegan?.Invoke(st.Current);
        }

        var seat = _seats[(int)st.Current];
        var actionSrc = _sources[(int)st.Current];
        seat.Tick(_runtime.Query, _runtime, actionSrc);
    }
}
