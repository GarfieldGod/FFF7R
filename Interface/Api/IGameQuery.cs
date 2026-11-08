namespace FFF7R
{
    // 只读查询通道，查询游戏内核状态
    public interface IGameQuery
    {
        GameStateView State { get; }

        // 当前棋盘全量快照。每次调用返回新实例，调用方可安全持有
        BoardSnapshot Board { get; }

        HandSnapshot Hand(PlayerSeat seat);

        // 预判这一步是否可下。<paramref name="reason"/> 在返回 false 时给出原因
        bool CanPlace(PlayerSeat seat, int handIndex, Int2 pos, out RejectReason reason);

        // 预览这一步下棋后的棋盘快照
        BoardSnapshot Preview(PlayerSeat seat, int handIndex, Int2 pos);

        // 是否有手牌可以下
        bool CanPlaceAny(PlayerSeat seat);
    }
}
