using System;
using System.Collections.Generic;

public class TestGame : Game {
    public TestGame(SingleGameConfig singleGameConfig) : base(singleGameConfig) {}

    static string FixLength(string input, int maxLength)
    {
        if (input.Length > maxLength)
        {
            return input.Substring(0, maxLength);
        }
        return input.PadRight(maxLength, ' ');
    }

    public void ShowCardInHand() {
        Log.TestLine("Cards:", TextColor.BLUE, true);
        int index = 0;
        foreach (var chess in player_.GetChessInHand()) {
            string name = FixLength(chess.GetChessProperty().Name, 10);
            Log.TestLine(
                index.ToString() + 
                "\t[" + name + "]" + 
                "\tCost: " + chess.GetChessProperty().Cost.ToString() + 
                "\tLevel: " + chess.GetChessProperty().Level.ToString()
            );
            index++;
        }
    }

    public void ShowChessGrid() {
        Log.TestLine("The ChessGrid:", TextColor.BLUE, true);
        int index = 0;
        for(int x = 0;x < chessPad_.GetChessGridStatus().Count ; x++) {
            for(int y = 0;y < chessPad_.GetChessGridStatus()[0].Count ; y++) {
                Chess chess = chessPad_.GetChessStatus()[x][y];
                int posStatus = chessPad_.GetChessGridStatus()[x][y];
                TextColor textColor;
                bool ifHightLight = true;
                if (posStatus > 10) {
                    textColor = TextColor.RED;
                } else if (posStatus < 10) {
                    textColor = TextColor.GREEN;
                } else {
                    textColor = TextColor.NONE;
                    ifHightLight = false;
                }

                string output = (posStatus % 10).ToString();
                if (chess != null) {
                    output = chess.GetChessProperty().Name;
                }
                Log.Test("(" + index.ToString("D2") + ")", TextColor.BLACK, false);
                Log.Test(FixLength(output, 15), textColor, ifHightLight);
                index++;
            }
            Log.Test("\n");
        }
    }

    Dictionary<int, Int2D> chessGridIndexMap_ = [];

    public override ChessPad InitChessPadStandard() {
        ChessPad chessPad = new ChessPad(
            new List<List<int>>{
                new List<int> { 1, 10, 10, 10, 11 },
                new List<int> { 1, 10, 10, 10, 11 },
                new List<int> { 1, 10, 10, 10, 11 },
            },
            new List<List<Chess>>{
                new List<Chess> { null, null, null, null, null },
                new List<Chess> { null, null, null, null, null },
                new List<Chess> { null, null, null, null, null },
            }
        );
        int index = 0;
        for(int x = 0;x < chessPad.GetChessGridStatus().Count ; x++) {
            for(int y = 0;y < chessPad.GetChessGridStatus()[0].Count ; y++) {
                chessGridIndexMap_.Add(index, new Int2D(x,y));
                index++;
            }
        }
        return chessPad;
    }

    public void ShowInfo(string info, TextColor textColor = TextColor.NONE) {
        // Log.Clear();
        ShowChessGrid();
        ShowCardInHand();
        Log.Test("Info:\n", TextColor.BLUE, true);
        Log.TestLine(info, textColor, true);
    }
    public override void RunGameTurns(int TimeEveryTurn) {
        while(gameStatus_ == GameStatus.GAMING) {
            base.RunGameTurns(TimeEveryTurn);
        }
    }
    public virtual void AiTurn() {
        // int delayTime = 3;
        // if (!rivalInputer_.IfCanDoInput()) {
        //     delayTime = 1;
        // }
        // if (Time.time - thisTurnStartTime > delayTime) {
        //     ChessInputParms chessInputParms = AiRival.GetTheBestInput(ChessPad.chessPadInfo_, rivalInputer_.GetChessInChessInHand());
        //     if (rivalInputer_.IfCanDoInput() && !chessInputParms.Empty()) {
        //         Log.TestLine("chessInputParms.Empty(): False");
        //         ChessInputParm chessInputParm = new ChessInputParm(
        //             chessInputParms,
        //             ChessPad.chessPadInfo_
        //         );
        //         rivalInputer_.GetChessInput(chessInputParm.chessInputParms.pos, chessInputParm.chessInputParms.cardCode, chessInputParm.chessPadInfo);
        //         ChessPad.CommitChessPadInfoToChessPad(ChessPad.chessPadInfo_);
        //     }
        //     NextTurn();
        // }
    }
    public override void RivalTurn() {

        base.RivalTurn();
    }
    public override void PlayerTurn()
    {
        PlayerTurnWithInfo("Select Card", TextColor.YELLOW);
        base.PlayerTurn();
    }

    private void PlayerTurnWithInfo(string info, TextColor textColor) {
        ShowInfo(info, textColor);

        string select = Console.ReadLine();
        if (int.TryParse(select, out int index) && index >= 0 && index < player_.GetChessInHand().Count &&
            (player_.Select(index) is { } selectedChess)) {

            ShowInfo(
                "Selected Card:" + selectedChess.GetChessProperty().Name +
                "\nSelect Grid To ADD:",
                TextColor.YELLOW
            );
            string input = Console.ReadLine();
            if (int.TryParse(input, out int grid) && grid >= 0 && grid < 15 && 
                player_.AddInput(new Input(chessGridIndexMap_[grid], selectedChess))) {

                ShowInfo(
                    "Selected Card: " + selectedChess.GetChessProperty().Name +
                    "\nSelected Grid: " + input +
                    "\nEnter 1 To Commit, 0 To Cancel:",
                    TextColor.YELLOW
                );
                string commit = Console.ReadLine();
                if (int.TryParse(commit, out int result) && result == 1) {
                    player_.CommitInput();
                } else {
                    player_.RestoreInput();
                    PlayerTurnWithInfo("Cancel Commit, Select Card:", TextColor.YELLOW);
                    return;
                }
            } else {
                player_.RestoreSelect();
                PlayerTurnWithInfo("Invaild Input! ReSelect Card!", TextColor.RED);
                return;
            }
        } else {
            PlayerTurnWithInfo("Invaild Input! ReSelect Card!", TextColor.RED);
            return;
        }
    }
}

#if !UNITY_ENGINE
class Program
{
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
        TestGame testGame = new TestGame(singleGameConfig);
        testGame.RunGameTurns(30);
    }
}
#endif