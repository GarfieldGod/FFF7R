using FFF7R;
using System;
using System.Collections.Generic;

public interface IGameView
{
    // 会话建立 / 重开 / resync。语义 = 「以当前快照为准」：全量拉取、清空待播队列
    void Bind(IGameQuery query);

    // 一次命令产生的完整事件集，按结算顺序到达。只入队，不播动画、不调命令
    void OnEvents(GameEventBatch batch);

    void Resync();

    // 会话销毁：解引用、停播放、归还池化对象
    void Unbind();
}

public sealed class GameEventBatch
{
    public IReadOnlyList<GameEvent> Events { get; }
    public int Count => Events.Count;
    // public T Find<T>() where T : GameEvent { /* 首个 T，没有返回 null */ }
    public GameEventBatch(IReadOnlyList<GameEvent> events) => Events = events;
}