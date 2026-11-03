using FFF7R;

interface ISeatPlayer
{
    PlayerSeat Seat { get; }
    void Tick(IGameQuery query, ISeatCommands commands, ISeatInputSource input);
}