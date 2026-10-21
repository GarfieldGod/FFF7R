namespace FFF7R
{
    using System.Collections.Generic;

    /// <summary>
    /// 写指令通道。全部同步执行，调用返回时状态与事件都已经落定。
    /// 失败不抛异常，用 <see cref="CommandResult"/> 返回。
    /// </summary>
    public interface IGameCommand
    {
        /// <summary>开局。</summary>
        CommandResult Start(PlayerSeat firstPlayer);

        /// <summary>出一张手牌到指定格。</summary>
        CommandResult Place(PlayerSeat seat, int handIndex, Int2 pos);

        /// <summary>重抽阶段保留指定的手牌，其余换掉。</summary>
        CommandResult ReDispense(PlayerSeat seat, IReadOnlyList<int> keepIndexes);
    }
}
