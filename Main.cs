using System;
using System.Collections.Generic;

class Program
{
    static readonly List<List<int>> posEffects = new List<List<int>>{
        new List<int>{ 0,  0,  0,  0,  0,  0,  0,  0,  0 },
        new List<int>{ 0,  0,  0,  0,  0,  2,  0,  0,  0 },
        new List<int>{ 0,  0,  0, -2,  9,  1, -1,  0,  0 },
        new List<int>{ 0,  0,  0,  0,  1,  0,  0,  0,  0 },
        new List<int>{ 0,  0,  0,  0,  0,  0,  0,  0,  0 }
    };

    static readonly List<List<int>> chessPad = new List<List<int>>{
        new List<int>{ 1,  0,  0,  0,  1 },
        new List<int>{ 1,  0,  0,  0,  1 },
        new List<int>{ 1,  0,  0,  0,  1 }
    };

    static void Main()
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
        Game testGame = new Game(singleGameConfig);
    }
}