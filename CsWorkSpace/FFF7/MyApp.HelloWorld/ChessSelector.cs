using System;

public class Chess {
    public int cost = 1;
    public int level = 1;
    public Tuple<int, int> chessPos = new Tuple<int, int>(0, 0);
}

public class ChessSelector {
    static Tuple<int, int> ChessSelectorPos = new Tuple<int, int>(0, 0);
    static int ChessSelectorLength = 10;
    static List<Chess> chessList = new List<Chess>{};

    public static void DoSendChess(Chess chessOne) {
        chessList.Add(chessOne);
        int chessGap = ChessSelectorLength / (chessList.Count + 1);
        Console.WriteLine("chessGap: " + chessGap);
        for(int i = 0; i < chessList.Count ; i++) {
            chessList[i].chessPos = new Tuple<int, int>(ChessSelectorPos.Item1 + (i + 1)*chessGap, ChessSelectorPos.Item2);
            Console.WriteLine("chess: " + " Pos: " + chessList[i].chessPos.Item1);
        }
        ResetAllChessPos();
    }

    public static void ResetAllChessPos() {
        Console.WriteLine("chessListSize: " + chessList.Count);
        foreach(var chess in chessList) {
            Console.WriteLine("chess: " + " Pos: " + chess.chessPos.Item1);
            for(int i = 0; i< chess.chessPos.Item1; i++) {
                Console.Write(" ");
            }
            // Console.Write(chess.chessPos.Item1);
            Console.Write(chess.level);
            Console.Write("\n");
        }
    }
}