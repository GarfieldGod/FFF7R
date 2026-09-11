namespace FFF7RCore.Test {
    public struct Step
    {
        public Input input;
        public ExpectPad expectPad;
        public Step(Input input, ExpectPad expectPad)
        {
            this.input = input;
            this.expectPad = expectPad;
        }
    }

    public class ExpectPad
    {
        public List<List<int>> PosStatus => expectGridMap_;
        public List<List<int>> LevelStatus => expectLevelMap_;
        List<List<int>> expectGridMap_;
        List<List<int>> expectLevelMap_;
        public ExpectPad(List<List<int>> chessGridStatus, List<List<int>> chessLevelStatus = null)
        {
            expectGridMap_ = chessGridStatus;
            expectLevelMap_ = chessLevelStatus;
        }
        public List<List<int>> GetExpectGridMap()
        {
            return expectGridMap_;
        }
        public List<List<int>> GetExpectLevelMap()
        {
            return expectLevelMap_;
        }
    }

    public static class TestUtils
    {
        public static void ShowStepInfo(Step step, int index)
        {
            Log.TestLine("\n\n------------StepInfo------------", TextColor.RED);
            Log.TestLine(
            "StepNum: " + index.ToString() + 
            "\nPlayer: " + step.input.playerType + 
            "\nInput Pos: " + step.input.pos + 
            "\nCardCode: " + step.input.chess.CardCode,
            TextColor.BLACK);
        }

        public static void ShowGameInfo(Game testGame)
        {
            Log.TestLine("------------GameInfo------------", TextColor.GREEN);
            Log.TestLine(
            "GameTurn: " + testGame.Turns + 
            " CurrentPlayer: " + testGame.CurrentPlayer,
            TextColor.BLACK);
            ShowGridLevel(testGame.ChessPad);
            ShowCardLevel(testGame.ChessPad);
            ShowChess(testGame.ChessPad);
        }

        public static void ShowErrorDiff(List<List<int>> expect, List<List<int>> result = null)
        {
            Log.TestLine("Expect:");
            if (expect == null)
            {
                Log.TestLine("The 2D List is null!");
                return;
            }
            else
            { 
                for (int x = 0; x < expect.Count; x++)
                {
                    for (int y = 0; y < expect[0].Count; y++)
                    {
                        Log.Test(Utils.FixLength(expect[x][y].ToString(), 15), result != null ? (expect[x][y] == result[x][y] ? TextColor.BLACK : TextColor.RED) : TextColor.PURPLE);
                    }
                    Log.Test("\n");
                }
            }
            Log.TestLine("Result:");
            if (result == null)
            {
                Log.TestLine("The 2D List is null!");
                return;
            }
            else
            { 
                for (int x = 0; x < result.Count; x++)
                {
                    for (int y = 0; y < result[0].Count; y++)
                    {
                        Log.Test(Utils.FixLength(result[x][y].ToString(), 15), expect[x][y] == result[x][y] ? TextColor.BLACK : TextColor.RED);
                    }
                    Log.Test("\n");
                }
            }
        }

        public static void ShowGridLevel(ChessPad chessPad)
        {
            Log.TestLine("The Grid Level:");
            for (int x = 0; x < chessPad.StatusMap.Count; x++)
            {
                for (int y = 0; y < chessPad.StatusMap[0].Count; y++)
                {
                    PadGrid grid = chessPad[x, y];
                    int posStatus = (int)grid.Status;
                    TextColor textColor;
                    if ((posStatus > 10 && posStatus < 14) || posStatus == 15)
                    {
                        textColor = TextColor.RED;
                    }
                    else if (posStatus < 10 || posStatus == 14)
                    {
                        textColor = TextColor.GREEN;
                    }
                    else
                    {
                        textColor = TextColor.NONE;
                    }

                    int level = posStatus % 10;

                    string output = level > 3 ? level == 4 ? "F" : "E" : level.ToString();
                    if (grid.Chess != null)
                    {
                        output = grid.Chess.Name;
                    }
                    Log.Test("(" + x.ToString() + ", " + y.ToString() + ")", TextColor.BLACK);
                    Log.Test(Utils.FixLength(output, 15), textColor);
                }
                Log.Test("\n");
            }
        }

        public static void ShowCardLevel(ChessPad chessPad)
        {
            Log.TestLine("The Card Level:");
            for (int x = 0; x < chessPad.CardLevelMap.Count; x++)
            {
                for (int y = 0; y < chessPad.CardLevelMap[0].Count; y++)
                {
                    PadGrid grid = chessPad[x, y];
                    int posStatus = (int)grid.Status;
                    int level = grid.Level;
                    TextColor textColor;
                    if ((posStatus > 10 && posStatus < 14) || posStatus == 15)
                    {
                        textColor = TextColor.RED;
                    }
                    else if (posStatus < 10 || posStatus == 14)
                    {
                        textColor = TextColor.GREEN;
                    }
                    else
                    {
                        textColor = TextColor.NONE;
                    }

                    string output = level.ToString();
                    Log.Test("(" + x.ToString() + ", " + y.ToString() + ")", TextColor.BLACK);
                    Log.Test(Utils.FixLength(output, 15), textColor);
                }
                Log.Test("\n");
            }
        }

        public static void ShowChess(ChessPad chessPad)
        {
            Log.TestLine("The Chess:");
            for (int x = 0; x < chessPad.StatusMap.Count; x++)
            {
                for (int y = 0; y < chessPad.StatusMap[0].Count; y++)
                {
                    PadGrid grid = chessPad[x, y];
                    int posStatus = (int)grid.Status;
                    TextColor textColor;
                    if ((posStatus > 10 && posStatus < 14) || posStatus == 15)
                    {
                        textColor = TextColor.RED;
                    }
                    else if (posStatus < 10 || posStatus == 14)
                    {
                        textColor = TextColor.GREEN;
                    }
                    else
                    {
                        textColor = TextColor.NONE;
                    }

                    string output = "null";
                    if (grid.Chess != null)
                    {
                        output = grid.Chess.Name;
                    }
                    Log.Test(Utils.FixLength(output, 15), textColor);
                }
                Log.Test("\n");
            }
        }
    }
}