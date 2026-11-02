using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FFF7R;
using System;

public class GameViewBinder : MonoBehaviour
{
    [SerializeField] GameBootstrap bootstrap;
    [SerializeField] List<MonoBehaviour> viewBehaviours;

    IGameSession _session;
    readonly List<IGameView> _gameViews = new();
    readonly List<GameEvent> _eventBuffer = new();
    long _lastSeq;
    bool _lost;

    void Awake()
    {
        foreach (var mb in viewBehaviours)
            if (mb is IGameView v) _gameViews.Add(v);
            else Debug.LogError($"{mb?.name} 未实现 IGameView");
        if (bootstrap != null) bootstrap.SessionCreated += Bind;

        if (bootstrap == null) { enabled = false; return; }
        bootstrap.SessionCreated += Bind;
        if (bootstrap.Session != null) Bind(bootstrap.Session);   // ← 关键：补一次回放
    }

    void OnEnable()   // 晚于全部 Awake，补一次回放，消除 Awake 顺序竞态
    {
        if (bootstrap != null && bootstrap.Session != null) Bind(bootstrap.Session);
    }

   void OnDestroy()
    {
        if (bootstrap != null) bootstrap.SessionCreated -= Bind;
        Unbind();
    }

    public CommandResult Execute(Func<CommandResult> command)   // 唯一的下命令出口
    {
        _eventBuffer.Clear();
        CommandResult r = command();
        if (_lost || _eventBuffer.Count != r.EventsRaised) Resync($"resync：事件数不符（实收 {_eventBuffer.Count}，声明 {r.EventsRaised}）");
        else if (_eventBuffer.Count > 0) Flush();
        return r;
    }

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
    }

    void Bind(IGameSession s)
    {
        Unbind();
        _session = s;
        _lastSeq = s.LastSequence;        // 基线：开局那批事件本来就没订阅，不该报跳号
        s.Raised += OnRaised;
        for (int i = 0; i < _gameViews.Count; i++) _gameViews[i].Bind(s);   // 隐式转成 IGameQuery
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
}

