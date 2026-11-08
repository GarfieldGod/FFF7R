using System;
using UnityEngine;
using FFF7R;
using FFF7R.Adapter;
using System.Collections.Generic;

public class GameBootstrap: IDisposable
{
    public event Action<IGameSession> SessionCreated;
    public event Action SessionDisposed;
    public IGameSession Session { get; private set; }

    private GameSetup _setup;

    public void Create(GameSetup setup)
    {
        _setup = setup;
        Dispose();

        Session = new GameSession(setup);
        SessionCreated?.Invoke(Session);
    }

    public void Restart(){}

    public bool ReDispense(List<int> playerDiscardIndexes, List<int> rivalDiscardIndexes)
    {
        CommandResult reDispense = Session.ReDispense(playerDiscardIndexes, rivalDiscardIndexes);
        if (!reDispense.Ok)
        {
            Debug.LogError($"重分配卡牌失败：{reDispense.Reason}（{reDispense}）");
            Dispose();
            return false;
        }
        return true;
    }

    public bool Start()
    {
        CommandResult started = Session.Start(_setup.FirstSeat);
        if (!started.Ok)
        {
            Debug.LogError($"开局失败：{started.Reason}（{started}）");
            Dispose();
            return false;
        }

        Debug.Log(
            $"开局成功 seed={Session.Seed} state={Session.State} " +
            $"手牌={Session.Hand(PlayerSeat.Player).Count} events={started.EventsRaised}");

        return true;
    }

    public void Dispose()
    {
        if (Session == null) return;
        Session.Dispose();
        Session = null;
        SessionDisposed?.Invoke();
    }
}
