using UnityEngine;
using FFF7R;
using System.Collections;

public class TurnCoordinator : MonoBehaviour
{
    [SerializeField] GameRuntime runtime;
    [SerializeField] ISeatPlayer player;
    [SerializeField] ISeatPlayer rival;
    public CardSelector cardSelector;

    IGameQuery _query;

    void Awake()
    {
        // player = new LocalSeatPlayer(PlayerSeat.Player, cardSelector);
        player = new AiSeatPlayer(PlayerSeat.Player);
        rival = new AiSeatPlayer(PlayerSeat.Rival);
    }

    void OnEnable()
    {
        runtime.SessionBound += OnBound;
    }

    void OnDisable() => runtime.SessionBound -= OnBound;

    void OnBound(IGameSession s) { _query = s;}

    void Update()
    {
        if (runtime == null || runtime.IsBusy) return;
        if (_query == null) _query = runtime.Query;
        if (_query == null) return;

        var st = _query.State;
        if (!st.IsGaming) return;

        var seat = st.Current == PlayerSeat.Player ? player : rival;
        seat.Tick(_query, runtime, null);
    }
}
