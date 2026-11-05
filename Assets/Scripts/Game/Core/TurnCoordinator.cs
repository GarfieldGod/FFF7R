using UnityEngine;
using FFF7R;
using System.Collections;

public class TurnCoordinator : MonoBehaviour
{
    [SerializeField] GameRuntime _runtime;
    SeatPlayer player;
    SeatPlayer rival;

    private ISeatActionSource _playerSource;
    private ISeatActionSource _rivalSource;
    public CardSelector cardSelector;
    public TurnBanner turnBanner;

    int _lastTurn = -1;

    void Awake()
    {
        player = new SeatPlayer(PlayerSeat.Player);
        rival = new SeatPlayer(PlayerSeat.Rival);

        _playerSource = new LocalPlayerActionSource(cardSelector);
        var aiSource = new AiActionSource();
        _rivalSource = new DelayedActionSource(aiSource, 3f);
    }

    void Update()
    {
        if (_runtime == null || _runtime.IsBusy) return;
        if (_runtime.Query == null) return;

        var st = _runtime.Query.State;
        if (!st.IsGaming) return;

        var seat = st.Current == PlayerSeat.Player ? player : rival;
        var actionSrc = st.Current == PlayerSeat.Player ? _playerSource : _rivalSource;
        seat.Tick(_runtime.Query, _runtime, actionSrc);

        st = _runtime.Query.State;
        if (!st.IsGaming) return;

        if (st.Turns != _lastTurn)
        {
            _lastTurn = st.Turns;
            turnBanner.Show(st.Current);
        }
    }
}
