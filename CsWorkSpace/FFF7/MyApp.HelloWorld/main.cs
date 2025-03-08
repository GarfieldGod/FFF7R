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
        Chess chess1 = new Chess();
        chess1.level = 1;
        Chess chess2 = new Chess();
        chess2.level = 2;
        Chess chess3 = new Chess();
        chess3.level = 3;
        Chess chess4 = new Chess();
        chess4.level = 4;
        Chess chess5 = new Chess();
        chess5.level = 5;
        Chess chess6 = new Chess();
        chess6.level = 6;
        Chess chess7 = new Chess();
        chess7.level = 7;
        ChessDispenser.chessPool = new List<Chess>{chess1, chess2, chess3, chess4, chess5, chess6, chess7};
        for(int i = 0;i < 5;i++) {
            ChessSelector.DoSendChess(ChessDispenser.DispenseChess());
        }
        List<int> chessIndex = new List<int>{ 1, 3, 4, 5 };
        ChessDispenser.ReDispense(chessIndex);
        // if(ChessSelector.DoPreview(chess3)) {
        //     ChessSelector.CancelPreview();
        //     ChessSelector.OutChess(chess3);
        // }
    }
}