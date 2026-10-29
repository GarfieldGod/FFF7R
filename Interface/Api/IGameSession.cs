namespace FFF7R
{
    using System;

    // 一局对外的全部能力。表现层只持有这一个引用，
    public interface IGameSession : IGameQuery, IGameCommand, IGameEvents, IDisposable
    {
        ICardCatalog Catalog { get; }

        // 本局实际使用的牌序种子。传 0 时使用随机生成的值
        int Seed { get; }
    }
}
