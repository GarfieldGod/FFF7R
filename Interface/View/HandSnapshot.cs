namespace FFF7R
{
    using System.Collections.Generic;

    /// <summary>某一方的手牌快照。</summary>
    public sealed class HandSnapshot
    {
        public PlayerSeat Owner { get; }
        public IReadOnlyList<CardView> Cards { get; }
        public int Count => Cards.Count;

        public static readonly HandSnapshot Empty =
            new HandSnapshot(PlayerSeat.None, new CardView[0]);

        public HandSnapshot(PlayerSeat owner, IReadOnlyList<CardView> cards)
        {
            Owner = owner;
            Cards = cards ?? new CardView[0];
        }

        public CardView this[int index] => Cards[index];

        public override string ToString() => Owner + " hand " + Count;
    }
}
