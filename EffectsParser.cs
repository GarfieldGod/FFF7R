using System;
using System.Collections.Generic;

public class EffectsParser
{
    public static readonly string[,] chessPos = {
        { "upLine0", "upLine1", "upLine2", "upLine3", "upLine4" },
        { "midLine0", "midLine1", "midLine2", "midLine3", "midLine4" },
        { "downLine0", "downLine1", "downLine2", "downLine3", "downLine4" }
    };

    public static List<Tuple<Tuple<int, int>, int>> ParseEffects(int[,] effects)
    {
        var midPos = new Tuple<int, int>((effects.GetLength(0) - 1) / 2, (effects.GetLength(1) - 1) / 2);
        var result = new List<Tuple<Tuple<int, int>, int>>();

        for (int i = 0; i < effects.GetLength(0); i++)
        {
            for (int j = 0; j < effects.GetLength(1); j++)
            {
                if (effects[i, j] != 0)
                {
                    int posY = i - midPos.Item1;
                    int posX = j - midPos.Item2;
                    if (posX != 0 || posY != 0)
                    {
                        var posWithValue = new Tuple<Tuple<int, int>, int>(new Tuple<int, int>(posY, posX), effects[i, j]);
                        result.Add(posWithValue);
                        Console.WriteLine($"y: {posY} x: {posX} value: {effects[i, j]}");
                    }
                }
            }
        }

        return result;
    }

    public static bool ParseEffectsInPosition(string posName, List<Tuple<Tuple<int, int>, int>> parsedEffects)
    {
        var posPos = new Tuple<int, int>(-1, -1);
        for (int i = 0; i < chessPos.GetLength(0); i++)
        {
            for (int j = 0; j < chessPos.GetLength(1); j++)
            {
                if (chessPos[i, j] == posName)
                {
                    posPos = new Tuple<int, int>(i, j);
                }
            }
        }

        if (posPos.Item1 < 0 || posPos.Item2 < 0)
        {
            return false;
        }

        Console.WriteLine($"posPos: {posPos.Item1} {posPos.Item2}");
        foreach (var effect in parsedEffects)
        {
            int posY = posPos.Item1 + effect.Item1.Item1;
            int posX = posPos.Item2 + effect.Item1.Item2;
            if (posY < chessPos.GetLength(0) && posY >= 0 && posX < chessPos.GetLength(1) && posX >= 0)
            {
                var affectedPos = new Tuple<int, int>(posY, posX);
                Console.WriteLine($"affectedPos: {chessPos[posY, posX]} \tpos: ({affectedPos.Item1}, {affectedPos.Item2}) effect: {effect.Item2}");
            }
        }

        return true;
    }
}