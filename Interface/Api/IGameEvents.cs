namespace FFF7R
{
    using System;

    /// <summary>
    /// 变更流通道。表现层只订阅这一个入口，不需要知道内核有几种 event。
    ///
    /// <para>
    /// 派发是**同步**的：事件在 <see cref="IGameCommand"/> 的调用栈里就被推出来，
    /// 所以订阅者不能在处理函数里再调 <see cref="IGameCommand"/>（会造成重入）。
    /// 需要“按序播放动画”时，处理函数只做入队，播放交给表现层自己的 Update 循环。
    /// </para>
    /// </summary>
    public interface IGameEvents
    {
        /// <summary>最近一次派发的事件序号；从未派发时为 0。</summary>
        long LastSequence { get; }

        /// <summary>每派发一个事件触发一次，参数按真实结算顺序到达。</summary>
        event Action<GameEvent> Raised;
    }
}
