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

    void Awake()
    {
        player = new SeatPlayer(PlayerSeat.Player);
        rival = new SeatPlayer(PlayerSeat.Rival);

        _playerSource = new LocalPlayerActionSource(cardSelector);
        _rivalSource = new AiActionSource();
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
    }
}
