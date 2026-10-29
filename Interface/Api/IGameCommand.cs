namespace FFF7R
{
    using System.Collections.Generic;

    // 上层使用的操作接口
    public interface IGameCommand
    {
        // 开局
        CommandResult Start(PlayerSeat firstPlayer);

        CommandResult PassTurn(PlayerSeat seat);

        // 出一张手牌到指定格
        CommandResult Place(PlayerSeat seat, int handIndex, Int2 pos);

        // 重抽阶段保留指定的手牌，其余换掉
        CommandResult ReDispense(PlayerSeat seat, IReadOnlyList<int> keepIndexes);
    }
}
