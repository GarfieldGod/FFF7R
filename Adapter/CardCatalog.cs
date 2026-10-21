namespace FFF7R.Adapter
{
    using System.Collections.Generic;
    using FFF7RCore;

    /// <summary>
    /// <see cref="ICardCatalog"/> 的内核实现：包住 <c>FFF7RCore.Property</c> 的静态卡牌库。
    ///
    /// 内核的 <c>GetChessProperty</c> 每次都返回同一批可变对象，直接交给表现层等于泄露内核类型；
    /// 所以这里在第一次访问时把全部定义转成不可变的 <see cref="CardDefView"/> 缓存起来。
    /// </summary>
    public sealed class CardCatalog : ICardCatalog
    {
        private readonly Dictionary<string, CardDefView> defs_ = new Dictionary<string, CardDefView>();
        private string[] codes_ = new string[0];
        private bool loaded_;

        /// <summary>加载内核卡牌库并重建缓存。可重复调用（热更后重载时用）。</summary>
        public void Load()
        {
            Property.LoadChessProperties();
            Rebuild();
        }

        /// <summary>内核卡牌库已经加载好时，只重建缓存，不重复读盘。</summary>
        public void Rebuild()
        {
            defs_.Clear();

            List<string> codes = Property.GetAllCardCode();
            codes_ = new string[codes.Count];
            for (int i = 0; i < codes.Count; i++)
            {
                string code = codes[i];
                codes_[i] = code;

                CardDefView def = KernelMapping.ToCardDef(Property.GetChessProperty(code));
                if (def != null) defs_[code] = def;
            }
            loaded_ = true;
        }

        public IReadOnlyList<string> AllCodes
        {
            get
            {
                EnsureLoaded();
                return codes_;
            }
        }

        public bool TryGet(string cardCode, out CardDefView def)
        {
            EnsureLoaded();
            if (string.IsNullOrEmpty(cardCode))
            {
                def = null;
                return false;
            }
            return defs_.TryGetValue(cardCode, out def);
        }

        public CardDefView Get(string cardCode)
        {
            CardDefView def;
            return TryGet(cardCode, out def) ? def : null;
        }

        private void EnsureLoaded()
        {
            if (!loaded_) Load();
        }
    }
}
