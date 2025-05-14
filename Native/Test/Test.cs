using System;
using System.Collections.Generic;

public class TestGame : Game {
    public TestGame(SingleGameConfig singleGameConfig) : base(singleGameConfig) {}
    public void ShowCardInHand() {
        Log.TestLine("Cards:", TextColor.BLUE, true);
        int index = 0;
        foreach (var chess in player_.GetSelector().GetChesses()) {
            string name = FixLength(chess.chessName_, 10);
            Log.TestLine(
                index.ToString() + 
                "\t[" + name + "]" + 
                "\tCost: " + chess.cost_.ToString() + 
                "\tLevel: " + chess.level_.ToString()
            );
            index++;
        }
    }
    static string FixLength(string input, int maxLength)
    {
        if (input.Length > maxLength)
        {
            return input.Substring(0, maxLength);
        }
        return input.PadRight(maxLength, ' ');
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
                if(chess != null) {
                    output = chess.chessName_;
                }
                Log.Test(output, textColor, ifHightLight);
                Log.Test("(" + index.ToString("D2") + ")   ", TextColor.BLACK, false);
                index++;
            }
            Log.Test("\n");
        }
    }
    public void ShowInfo(string info, TextColor textColor = TextColor.NONE) {
        Log.Clear();
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
    public override void PlayerTurn()
    {
        PlayerTurnWithInfo("Select Card", TextColor.YELLOW);
        base.PlayerTurn();
    }

    private void PlayerTurnWithInfo(string info, TextColor textColor) {
        ShowInfo(info, textColor);

        string select = Console.ReadLine();
        if (int.TryParse(select, out int index) && index >= 0 && index < player_.GetSelector().GetAllChessNum()) {
            Chess selectedChess = player_.GetSelector().GetChess(index);
            player_.GetSelector().Preview(index);
            ShowInfo(
                "Selected Card:" + selectedChess.chessName_ +
                "\nSelect Grid To ADD:",
                TextColor.YELLOW
            );

            string input = Console.ReadLine();
            if (int.TryParse(input, out int grid) && grid >= 0 && grid < 15) {
                ShowInfo(
                    "Selected Card: " + selectedChess.chessName_ +
                    "\nSelected Grid: " + input +
                    "\nEnter 1 To Commit, 0 To Cancel:",
                    TextColor.YELLOW
                );
                string commit = Console.ReadLine();
                if (int.TryParse(commit, out int result) && result == 1) {
                    
                    player_.GetSelector().Commit();
                } else {
                    player_.GetSelector().CancelPreview();
                    PlayerTurnWithInfo("Cancel Commit, Select Card:", TextColor.YELLOW);
                    return;
                }
            } else {
                player_.GetSelector().CancelPreview();
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
        testGame.ShowChessGrid();
        testGame.ShowCardInHand();
        testGame.RunGameTurns(30);
    }
}
#endif