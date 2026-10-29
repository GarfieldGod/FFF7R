namespace FFF7R
{
    using System.Collections.Generic;

    // 卡牌目录，卡牌定义的静态数据源
    public interface ICardCatalog
    {
        void Load();
        IReadOnlyList<string> AllCodes { get; }

        bool TryGet(string cardCode, out CardDefView def);
        CardDefView Get(string cardCode);
    }
}
