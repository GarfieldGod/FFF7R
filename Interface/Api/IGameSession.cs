namespace FFF7R
{
    using System;

    /// <summary>
    /// 一局对外的全部能力。表现层只持有这一个引用，
    /// 其余内核类型（Game / ChessPad / ChessProperty / Int2D）一律不可见。
    /// </summary>
    public interface IGameSession : IGameQuery, IGameCommand, IGameEvents, IDisposable
    {
        ICardCatalog Catalog { get; }
    }
}
