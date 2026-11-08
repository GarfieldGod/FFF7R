using System;
using UnityEngine;
using FFF7R;
using FFF7R.Adapter;

public class GameBootstrap: IDisposable
{
    public event Action<IGameSession> SessionCreated;
    public IGameSession Session { get; private set; }

    public void Start(GameSetup setup)
    {
        Dispose();

        Session = new GameSession(setup);

        CommandResult started = Session.Start(setup.FirstSeat);
        if (!started.Ok)
        {
            Debug.LogError($"开局失败：{started.Reason}（{started}）");
            Dispose();
            return;
        }

        Debug.Log(
            $"开局成功 seed={Session.Seed} state={Session.State} " +
            $"手牌={Session.Hand(PlayerSeat.Player).Count} events={started.EventsRaised}");

        SessionCreated?.Invoke(Session);
    }

    public void Dispose()
    {
        if (Session == null) return;
        Session.Dispose();
        Session = null;
    }
}
