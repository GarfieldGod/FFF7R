namespace FFF7R
{
    /// <summary>
    /// 手牌里的一张牌。<see cref="HandIndex"/> 是对局内唯一的稳定下标，
    /// 指令层用它来指定出哪张牌，避免表现层持有可能失效的对象引用。
    /// </summary>
    public sealed class CardView
    {
        public int HandIndex { get; }
        public string CardCode { get; }
        public CardDefView Def { get; }

        public CardView(int handIndex, string cardCode, CardDefView def)
        {
            HandIndex = handIndex;
            CardCode = cardCode;
            Def = def;
        }

        public override string ToString() => "#" + HandIndex + " " + CardCode;
    }
}
