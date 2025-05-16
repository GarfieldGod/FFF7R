using System;
using System.Collections.Generic;

#if !UNITY_ENGINE
class Program
{
    static void Main()
    {
        Test.TestCase1 testCase1 = new Test.TestCase1();
        testCase1.Run();
        // StartConsoleGame();
    }

    static void StartConsoleGame()
    {
        Property.LoadChessProperties();
        SingleGameConfig singleGameConfig = new SingleGameConfig(
            1,
            new List<string>{
                "CardTest0", "CardTest0", "CardTest0", "CardTest0",
                "CardTest1", "CardTest1", "CardTest1", "CardTest1",
                "Card006", "Card006", "Card006", "Card006",
                "Card009", "Card009", "Card009",
            },
            new List<string>{
                "Card001", "Card001", "Card001",
                "Card007", "Card007", "Card007", "Card007",
                "Card008", "Card008", "Card008", "Card008",
                "Card009", "Card009", "Card009", "Card009",
            }
        );
        ConsoleGame consoleGame = new ConsoleGame(singleGameConfig);
        consoleGame.RunGameTurns(30);
    }
}
#endif