#if !UNITY_ENGINE
namespace FFF7RCore{
    using Test;

    class Program
    {
        static int Main()
        {
            return SystemTest.Run();
        }
    }

    //     static void StartConsoleGame()
    //     {
    //         Property.LoadChessProperties();
    //         GameConfig GameConfig = new GameConfig(
    //             1,
    //             new List<string>{
    //                 "PosAll4Down1", "PosAll4Down1", "PosAll4Down1", "PosAll4Down1",
    //                 "PosFriend4Up1", "PosFriend4Up1", "PosFriend4Up1", "PosFriend4Up1",
    //                 "Card006", "Card006", "Card006", "Card006",
    //                 "Card009", "Card009", "Card009",
    //             },
    //             new List<string>{
    //                 "Card001", "Card001", "Card001",
    //                 "Card007", "Card007", "Card007", "Card007",
    //                 "Card008", "Card008", "Card008", "Card008",
    //                 "Card009", "Card009", "Card009", "Card009",
    //             }
    //         );
    //         ConsoleGame consoleGame = new ConsoleGame(GameConfig);
    //         consoleGame.RunGameTurns(30);
    //     }
}
#endif