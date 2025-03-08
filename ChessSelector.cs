using System;
using System.Collections.Generic;

public class Chess {
    public int cost = 1;
    public int level = 1;
    public Tuple<int, int> chessPos = new Tuple<int, int>(0, 0);
}

public class ChessDispenser {
    public static List<Chess> chessPool = new List<Chess>{};

    public static Chess DispenseChess() {
        if (chessPool.Count == 0) {
            return null;
        }
        Random random = new Random();
        int randomIndex = random.Next(chessPool.Count);
        Chess chess = chessPool[randomIndex];
        chessPool.RemoveAt(randomIndex);
        return chess;
    }

    public static void ReDispense(List<int> index) {
        List<Chess> popChessList = new List<Chess>{};
        foreach(int i in index) {
            if(i >= ChessSelector.chessList.Count) {
                Console.WriteLine("Invaild chessIndex.");
                continue;
            }
            popChessList.Add(ChessSelector.chessList[i]);
        }
        foreach(Chess chess in popChessList) {
            ChessSelector.chessList.Remove(chess);
        }
        foreach(Chess chess in popChessList) {
            ChessSelector.DoSendChess(DispenseChess());
        }
        foreach(Chess chess in popChessList) {
            chessPool.Add(chess);
        }
        Console.WriteLine("ReDispense Over.");
    }
}

public class ChessSelector {
    static Tuple<int, int> ChessSelectorPos = new Tuple<int, int>(0, 0);
    static int ChessSelectorLength = 100;
    public static List<Chess> chessList = new List<Chess>{};
    public static KeyValuePair<Chess?, int> previewChess = new KeyValuePair<Chess?, int>(null, 0);

    public static void DoSendChess(Chess chessOne) {
        if(chessOne == null) {
            return;
        }
        chessList.Add(chessOne);
        ResetAllChessPos();
    }

    public static void ResetAllChessPos() {
        int chessGap = ChessSelectorLength / (chessList.Count + 1);
        Console.WriteLine("chessGap: " + chessGap);
        for (int i = 0; i < chessList.Count; i++) {
            chessList[i].chessPos = new Tuple<int, int>(ChessSelectorPos.Item1 + (i + 1) * chessGap, ChessSelectorPos.Item2);
            // Console.WriteLine("DoSendChess chess: " + i + " Pos: " + chessList[i].chessPos.Item1);
        }
        Console.WriteLine("chessListSize: " + chessList.Count);
        foreach (var chess in chessList) {
            //Console.WriteLine("ResetAllChessPos chess Pos: " + chess.chessPos.Item1);
            for (int i = 0; i < chess.chessPos.Item1; i++) {
                Console.Write(" ");
            }
            Console.Write(chess.level);
            Console.Write("\n");
        }
    }

    public static bool OutChess(Chess chess) {
        bool result = chessList.Remove(chess);
        ResetAllChessPos();
        return result;
    }

    public static bool DoPreview(Chess chess) {
        Console.WriteLine("------------------------------DoPreview");
        previewChess = new KeyValuePair<Chess?, int>(chess, chessList.IndexOf(chess));
        bool result = chessList.Remove(chess);
        ResetAllChessPos();
        return result;
    }

    public static void CancelPreview() {
        Console.WriteLine("------------------------------CancelPreview");
        if (previewChess.Key == null) {
            Console.WriteLine("CancelPreview Nothing to do.");
            return;
        }
        chessList.Insert(previewChess.Value, previewChess.Key);
        previewChess = new KeyValuePair<Chess?, int>(null, 0);
        ResetAllChessPos();
    }
}