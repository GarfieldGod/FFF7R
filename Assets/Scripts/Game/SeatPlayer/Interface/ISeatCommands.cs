using FFF7R;

public interface ISeatCommands
{
    CommandResult Place(PlayerSeat seat, int handIndex, Int2 pos);
    CommandResult Pass(PlayerSeat seat);
}