using UnityEngine;
using FFF7R;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public sealed class GameHost : MonoBehaviour
{
    [Header("对局参数")]
    [SerializeField] private int boardWidth = 3;
    [SerializeField] private int boardHeight = 5;
    [SerializeField] private int turnLimit = 0;
    [SerializeField] private int seed = 0;

    [Header("视图")]
    [SerializeField] private CardBoard board;
    [SerializeField] private CardSelector[] hands;
    [SerializeField] private ResultPanel resultPanel;
    [SerializeField] private GamingPanel gamingPanel;
    [SerializeField] private ReDispensePanel reDispensePanel;

    private GameBootstrap _boot;
    private GameRuntime _runtime;
    private TurnCoordinator _turn;

    public GameSetup TestStartLocalGame()
    {
        string[] DefaultPlayerPool =
        {
            "Card001", 
            "Card002", "Card003", "Card004", "Card005",
            "Card006", "Card007", "Card008", "Card009", "Card010",
        };
        string[] DefaultRivalPool =
        {
            "Card011", "Card012", "Card013", "Card014", "Card015",
            "Card016", "Card017", "Card018", "Card019", "Card020",
        };
        GameSetup setup = new GameSetup(
            playerPool: DefaultPlayerPool,
            rivalPool: DefaultRivalPool,
            boardWidth: boardWidth,
            boardHeight: boardHeight,
            standardBoard: true,
            firstSeat: PlayerSeat.Player,
            turnLimit: turnLimit,
            seed: seed
        );

        PreLoadCardAssets(setup.PlayerPool, setup.RivalPool);
        return setup;
    }

    private static void PreLoadCardAssets(IReadOnlyList<string> PlayerPool, IReadOnlyList<string> RivalPool)
    {
        foreach (var code in PlayerPool) GameAssets.Sprites.Request(code, _ => { });
        foreach (var code in RivalPool)  GameAssets.Sprites.Request(code, _ => { });
    }

    void Awake()
    {
        FFF7RCore.Log.PrintToConsole = false;

        var localAct = new LocalPlayerActionSource(hands[0]);
        var aiAct = new DelayedActionSource(new AiActionSource(hands[1]), 3f);

        _boot = new GameBootstrap();
        _runtime = new GameRuntime();
        _turn    = new TurnCoordinator(_runtime, new ISeatActionSource[] {null, localAct, aiAct});

        _runtime.Register(board);
        _runtime.Register(hands[0]);
        _runtime.Register(hands[1]);

        _boot.SessionCreated += _runtime.Bind;
        _boot.SessionDisposed += _runtime.Dispose;

        _turn.TurnBegan += gamingPanel.OnTurnBegin;
        _turn.MatchEnded += gamingPanel.OnMatchEnd;
        _turn.MatchEnded += resultPanel.OnMatchEnd;

        gamingPanel.PassRequested += localAct.RequestPass;
        reDispensePanel.ReDispenseRequested += SubmitReDispense;

        _boot.Create(TestStartLocalGame());
        reDispensePanel.OnStageChanged(MatchStatus.ReDispense);
    }

    public void SubmitReDispense()
    {
        if (_boot.Session == null) return;
        if (_boot.Session.State.Status != MatchStatus.ReDispense) return;

        if (!_boot.ReDispense(
            hands[0].GetReDispense(), 
            new List<int>() // AI 先不换
            )) 
            return;

        hands[0].ClearReDispense();
        _runtime.Refresh();        // ReDispense 不 raise 事件，必须手动刷新
        if (!_boot.Start()) return;

        reDispensePanel.OnStageChanged(MatchStatus.Gaming);
    }

    void Update() => _turn.Tick();          // 逻辑：先跑
    // LateUpdate 留给 view（CardSelector / CardMotion）
    void OnDestroy() { _boot.Dispose(); _runtime.Dispose(); }
}
