namespace FFF7R
{
    /// <summary>
    /// 卡牌静态定义（只读视图）。对应内核的 ChessProperty，但剥掉了 PosOffsetDict 之类的内部缓存。
    /// 由 <see cref="ICardCatalog"/> 提供，生命周期与对局无关，可缓存。
    /// </summary>
    public sealed class CardDefView
    {
        public string CardCode { get; }
        public string Name { get; }

        /// <summary>基础战力（卡面数字）。</summary>
        public int Level { get; }

        /// <summary>费用，表现为卡面上的费用点数量。</summary>
        public int Cost { get; }

        public string Description { get; }

        /// <summary>放置影响范围（卡面 5x5 网格里高亮的那部分）。</summary>
        public GridPattern PosEffect { get; }

        /// <summary>卡牌效果范围，可能为 null。</summary>
        public GridPattern EffectScope { get; }

        /// <summary>效果的人读摘要，仅用于 tooltip；null 表示没有额外效果。</summary>
        public string EffectSummary { get; }

        public CardDefView(
            string cardCode,
            string name,
            int level,
            int cost,
            string description,
            GridPattern posEffect,
            GridPattern effectScope,
            string effectSummary)
        {
            CardCode = cardCode;
            Name = name;
            Level = level;
            Cost = cost;
            Description = description;
            PosEffect = posEffect ?? GridPattern.Empty;
            EffectScope = effectScope;
            EffectSummary = effectSummary;
        }
    }
}
