using System;
using UnityEngine;
using FFF7R;
using FFF7R.Adapter;

/// <summary>
/// 对局入口（composition root）：负责构造 GameSession 并把它的生命周期管住。
///
/// 这一层只做三件事：组 GameSetup、new GameSession、按需 Dispose。
/// 其它任何地方都不要再 new 会话，也不要碰 FFF7RCore。
/// </summary>
public class GameBootstrap : MonoBehaviour
{
    [Header("对局参数")]
    [SerializeField] private int boardWidth = 3;
    [SerializeField] private int boardHeight = 5;
    [SerializeField] private int turnLimit = 0;
    [SerializeField] private int seed = 0;

    private IGameSession session_;

    public IGameSession Session => session_;
    public event Action<IGameSession> SessionCreated;

    private void Awake()
    {
        FFF7RCore.Log.PrintToConsole = false;
        StartGame();
    }

    private void OnDestroy()
    {
        DisposeSession();
    }

    public void StartGame()
    {
        DisposeSession();

        GameSetup setup = new GameSetup(
            playerPool: DefaultPlayerPool,
            rivalPool: DefaultRivalPool,
            boardWidth: boardWidth,
            boardHeight: boardHeight,
            standardBoard: true,
            firstSeat: PlayerSeat.Player,
            turnLimit: turnLimit,
            seed: seed);

        session_ = new GameSession(setup);

        CommandResult started = session_.Start(setup.FirstSeat);
        if (!started.Ok)
        {
            Debug.LogError($"开局失败：{started.Reason}（{started}）");
            DisposeSession();
            return;
        }

        Debug.Log(
            $"开局成功 seed={session_.Seed} state={session_.State} " +
            $"手牌={session_.Hand(PlayerSeat.Player).Count} events={started.EventsRaised}");

        SessionCreated?.Invoke(session_);
    }

    private void DisposeSession()
    {
        if (session_ == null) return;

        session_.Dispose();
        session_ = null;
    }

    private static readonly string[] DefaultPlayerPool =
    {
        "Card001", "Card002", "Card003", "Card004", "Card005",
        "Card006", "Card007", "Card008", "Card009", "Card010",
    };

    private static readonly string[] DefaultRivalPool =
    {
        "Card011", "Card012", "Card013", "Card014", "Card015",
        "Card016", "Card017", "Card018", "Card019", "Card020",
    };
}
