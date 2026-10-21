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

        /// <summary>
        /// 本局实际使用的牌序种子。<see cref="GameSetup.Seed"/> 传 0 时这里会给出随机生成的值，
        /// 记下它就能用同一个种子重放这一局（崩溃复现、回放、录像都靠它）。
        /// </summary>
        int Seed { get; }
    }
}
