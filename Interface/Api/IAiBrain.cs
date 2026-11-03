namespace FFF7R
{
    public interface IAiBrain
    {
        bool TryDecide(IGameQuery query, PlayerSeat seat, out AiDecision decision);
    }

    public readonly struct AiDecision
    {
        public readonly int HandIndex;
        public readonly Int2 Pos;
        public AiDecision(int handIndex, Int2 pos) { HandIndex = handIndex; Pos = pos; }
    }
}