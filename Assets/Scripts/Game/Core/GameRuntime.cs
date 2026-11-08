using System;
using System.Collections.Generic;
using FFF7R;
using UnityEngine; // Debug.Log()

public class GameRuntime : ISeatCommands, IDisposable
{
    readonly List<IGameView> _gameViews = new();
    readonly List<GameEvent> _eventBuffer = new();
    IGameSession _session;
    long _lastSeq;
    bool _lost;

    public bool IsBusy;
    public IGameQuery Query => _session;
    public event Action<IGameSession> SessionBound;
    public event Action EventFlushed;

    public void Register(IGameView v)
    {
        if (v == null || _gameViews.Contains(v)) return;
        _gameViews.Add(v);
        if (_session != null) SafeBind(v);        // 已有会话就补一次
    }

    public void Unregister(IGameView v) => _gameViews.Remove(v);

    public CommandResult Execute(Func<IGameCommand, CommandResult> op)
    {
        IsBusy = true;
        _eventBuffer.Clear();
        try {
            CommandResult r = op(_session);          // 调用方拿不到 session，只能在这一帧里用
            if (_lost || _eventBuffer.Count != r.EventsRaised) Resync($"resync：事件数不符（实收 {_eventBuffer.Count}，声明 {r.EventsRaised}）");
            else if (_eventBuffer.Count > 0) Flush();
            return r;
        }
        finally { IsBusy = false; }
    }

    public void Execute(Action<IGameCommand> op) => Execute(c => { op(c); return CommandResult.Success(); });

    public CommandResult Place(PlayerSeat seat, int handIndex, Int2 pos) => Execute(c => c.Place(seat, handIndex, pos));

    public CommandResult Pass(PlayerSeat seat) => Execute(c => c.PassTurn(seat));

    void Resync(string why)
    {
        Debug.LogWarning(why);
        if (_session == null) return;
        Bind(_session);   // Bind 第一句就是 Unbind → 退订、清队列、全量重拉快照
    }

    void OnRaised(GameEvent e)
    {
        if (_lost) return;
        if (e.Sequence != _lastSeq + 1) { _lost = true; return; }   // 别在派发栈里重建会话
        _lastSeq = e.Sequence;
        _eventBuffer.Add(e);
    }

    void Flush()
    {
        var batch = new GameEventBatch(_eventBuffer.ToArray());
        for (int i = 0; i < _gameViews.Count; i++)
            try { _gameViews[i].OnEvents(batch); }
            catch (Exception ex) { Debug.LogException(ex); }        // 一个视图炸不能截断其它视图
        _eventBuffer.Clear();
        EventFlushed?.Invoke();
    }

    public void Refresh()
    {
        if (_session == null) return;
        for (int i = 0; i < _gameViews.Count; i++)
            try { _gameViews[i].Resync(); }
            catch (Exception ex) { Debug.LogException(ex); }
    }

    public void Bind(IGameSession s)
    {
        Unbind();
        _session = s;
        _lastSeq = s.LastSequence;        // 基线：开局那批事件本来就没订阅，不该报跳号
        s.Raised += OnRaised;
        for (int i = 0; i < _gameViews.Count; i++) SafeBind(_gameViews[i]);   // 隐式转成 IGameQuery
        SessionBound?.Invoke(_session);
    }

    void Unbind()
    {
        if (_session == null) return;
        _session.Raised -= OnRaised;
        _session = null;
        _eventBuffer.Clear();
        _lost = false;
        for (int i = 0; i < _gameViews.Count; i++)
            try { _gameViews[i].Unbind(); }
            catch (Exception ex) { Debug.LogException(ex); }
    }

    void SafeBind(IGameView v)
    {
        try { v.Bind(_session); } catch (Exception ex) { Debug.LogException(ex); }
    }

    public void Dispose() => Unbind();
}

