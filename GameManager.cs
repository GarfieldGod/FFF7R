using System;
using System.Collections.Generic;

public class GameManager {
    public static List<List<int>> chessGridLevel = new List<List<int>>{
        new List<int> { 0, 0, 0, 0, 0 },
        new List<int> { 0, 0, 0, 0, 0 },
        new List<int> { 0, 0, 0, 0, 0 },
    };

    public static List<List<int>> chessGridStatus = new List<List<int>>{
        new List<int> { 1, 10, 10, 10, 11 },
        new List<int> { 1, 10, 10, 10, 11 },
        new List<int> { 1, 10, 10, 10, 11 },
    };

    public static void StartAGame() {
        ChessDispenser.chessPool = new List<Chess>{};
        for(int i = 0; i < 15; i++) {
            Chess chess = new Chess();
            chess.level = i + 1;
            ChessDispenser.chessPool.Add(chess);
        }
        for(int i = 0;i < 5;i++) {
            ChessSelector.DoSendChess(ChessDispenser.DispenseChess());
        }
        ReDisChess();
        ShowChessGrid();
        ShowChessInChessSelector();
    }

    public static void ReDisChess() {
        ShowChessInChessSelector();
        Console.WriteLine("Input Card Index to ReDispense The Cards:");
        string input = Console.ReadLine();
        if (input != "skip") {
            string[] numberStrings = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            List<int> chessIndex = new List<int>{};
            foreach (string numberString in numberStrings)
            {
                if (int.TryParse(numberString, out int number)) {
                    chessIndex.Add(number);
                } else {
                    Console.WriteLine("Invaild Chess Index: " + numberString);
                }
            }
            ChessDispenser.ReDispense(chessIndex);
        }
    }

    public static void repeatTurns() {
        string input = "";
        while(input != "end") {

        }
    }

    public static void ShowChessInChessSelector() {
        Console.WriteLine("Cards:");
        foreach (var chess in ChessSelector.chessList) {
            Console.Write(" ");
            Console.Write(chess.level);
        }
        Console.Write("\n");
    }

    public static void ShowChessGrid() {
        Console.WriteLine("The ChessGrid:");
        foreach(List<int> gridLine in chessGridStatus) {
            foreach(int grid in gridLine) {
                Console.Write(grid.ToString() + " ");
            }
            Console.Write("\n");
        }
    }
}