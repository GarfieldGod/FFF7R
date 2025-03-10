using System;
using System.Collections.Generic;

class Program
{
    static readonly int[,] posEffects = {
        { 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 2, 0, 0, 0 },
        { 0, 0, 0, -2, 9, 1, -1, 0, 0 },
        { 0, 0, 0, 0, 1, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 0, 0, 0 }
    };

    static void Main()
    {
        // string posName = "midLine1";
        // var parsedEffects = EffectsParser.ParseEffects(posEffects);
        // bool parseSucc = EffectsParser.ParseEffectsInPosition(posName, parsedEffects);
        GameManager.StartAGame();
        // if(ChessSelector.DoPreview(chess3)) {
        //     ChessSelector.CancelPreview();
        //     ChessSelector.OutChess(chess3);
        // }
    }
}