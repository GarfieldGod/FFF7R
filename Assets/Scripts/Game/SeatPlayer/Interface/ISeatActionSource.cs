using FFF7R;

public interface ISeatActionSource
{
    bool TryTake(IGameQuery query, PlayerSeat seat, out SeatAction action);
    void OnResult(CommandResult result);
}

public struct SeatAction
{
    public enum Kind
    {
        None,
        Place,
        Pass
    }

    public readonly Kind ActionKind;
    public readonly int HandIndex;
    public readonly Int2 Pos;

    private SeatAction(Kind kind, int handIndex, Int2 pos)
    { 
        ActionKind = kind;
        HandIndex = handIndex;
        Pos = pos;
    }

    public static SeatAction Place(int handIndex, Int2 pos) => new(Kind.Place, handIndex, pos);
    public static SeatAction Pass() => new(Kind.Pass, -1, default);

    public static readonly SeatAction None = new(Kind.None, -1, Int2.Unknown);
}