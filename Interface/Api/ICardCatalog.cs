namespace FFF7R
{
    using System.Collections.Generic;

    /// <summary>
    /// 卡牌定义的静态数据源。与对局无关，加载一次即可反复使用。
    /// </summary>
    public interface ICardCatalog
    {
        /// <summary>加载（或重新加载）卡牌定义。可重复调用。</summary>
        void Load();

        IReadOnlyList<string> AllCodes { get; }

        bool TryGet(string cardCode, out CardDefView def);

        /// <summary>查不到时返回 null，不抛异常。</summary>
        CardDefView Get(string cardCode);
    }
}
