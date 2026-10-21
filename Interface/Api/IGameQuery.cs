namespace FFF7R
{
    /// <summary>
    /// 只读查询通道。全部是同步的纯读操作，**不允许有副作用**——
    /// 表现层可以在同一帧里任意调用（预览、按钮灰度、tooltip）。
    /// </summary>
    public interface IGameQuery
    {
        GameStateView State { get; }

        /// <summary>当前棋盘全量快照。每次调用返回新实例，调用方可安全持有。</summary>
        BoardSnapshot Board { get; }

        HandSnapshot Hand(PlayerSeat seat);

        /// <summary>
        /// 预判这一步是否可下。<paramref name="reason"/> 在返回 false 时给出原因。
        /// 与 <see cref="IGameCommand.Place"/> 共用同一套判定，保证“能预览就能下”。
        /// </summary>
        bool CanPlace(PlayerSeat seat, int handIndex, Int2 pos, out RejectReason reason);

        /// <summary>
        /// 试算：把这张牌放在这里之后棋盘会变成什么样。用于悬停高亮。
        /// 不改变对局状态、不派发事件。非法落子返回 null。
        /// </summary>
        BoardSnapshot Preview(PlayerSeat seat, int handIndex, Int2 pos);
    }
}
