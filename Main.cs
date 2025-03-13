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
        Int2D chessPos = new Int2D(2, 0);
        var result = ChessInputer.DoPosEffect(chessPos, posEffects, chessPad);
        foreach(var line in result) {
            foreach(var i in line) {
                Console.Write("\t" + i.ToString());
            }
            Console.Write("\n");
        }
        // GameManager.StartAGame();
        // if(ChessSelector.DoPreview(chess3)) {
        //     ChessSelector.CancelPreview();
        //     ChessSelector.OutChess(chess3);
        // }
    }
}