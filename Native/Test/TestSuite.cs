using System.ComponentModel;
namespace Test
{
    public class TestSuite
    {
        private ChessPad chessPad_;
        private List<Step> stepList_;
        public TestSuite(ChessPad chessPad)
        {
            chessPad_ = chessPad;
            stepList_ = new List<Step> { };
        }
        public void AddStep(int index, string cardCode, InputerType inputerType, ExpectPad expectPad)
        {
            stepList_.Add(new Step(inputerType, index, cardCode, expectPad));
        }
        public void AddStep(Step step)
        {
            stepList_.Add(step);
        }
        public bool Run()
        {
            Property.LoadChessProperties();
            SingleGameConfig singleGameConfig = new SingleGameConfig(1, new List<string> { }, new List<string> { });
            TestGame testGame = new TestGame(stepList_, singleGameConfig);
            testGame.RunGameTurns(30);
            return testGame.TestResult;
        }
    }

    public class ExpectPad
    {
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

    public struct Step
    {
        public int index;
        public string cardCode;
        public InputerType inputerType;
        public ExpectPad expectPad;
        public Step(InputerType inputerType, int index, string cardCode, ExpectPad expectPad)
        {
            this.index = index;
            this.cardCode = cardCode;
            this.inputerType = inputerType;
            this.expectPad = expectPad;
        }
    }

    public class TestGame : Game
    {
        List<Step> stepList_;
        public bool TestResult = false;
        public TestGame(List<Step> steps, SingleGameConfig singleGameConfig) : base(singleGameConfig)
        {
            this.stepList_ = steps;
        }

        public static void ShowErrorDiff(List<List<int>> expcet, List<List<int>> result = null)
        {
            Log.TestLine("Expect:");
            if (expcet == null)
            {
                Log.TestLine("The 2D List is null!");
                return;
            }
            else
            { 
                for (int x = 0; x < expcet.Count; x++)
                {
                    for (int y = 0; y < expcet[0].Count; y++)
                    {
                        Log.Test(Utils.FixLength(expcet[x][y].ToString(), 15), result != null ? (expcet[x][y] == result[x][y] ? TextColor.BLACK : TextColor.RED) : TextColor.PURPLE);
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
                        Log.Test(Utils.FixLength(result[x][y].ToString(), 15), expcet[x][y] == result[x][y] ? TextColor.BLACK : TextColor.RED);
                    }
                    Log.Test("\n");
                }
            }
        }

        public void ShowChessGrid()
        {
            Log.TestLine("The ChessGrid:");
            int index = 0;
            for (int x = 0; x < chessPad_.GetGridStatusMap().Count; x++)
            {
                for (int y = 0; y < chessPad_.GetGridStatusMap()[0].Count; y++)
                {
                    Chess chess = chessPad_.GetChessMap()[x][y];
                    int posStatus = chessPad_.GetGridStatusMap()[x][y];
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

                    string output = (posStatus % 10).ToString();
                    if (chess != null)
                    {
                        output = chess.GetChessProperty().Name;
                    }
                    Log.Test("(" + index.ToString("D2") + ")", TextColor.BLACK);
                    Log.Test(Utils.FixLength(output, 15), textColor);
                    index++;
                }
                Log.Test("\n");
            }
        }

        public void ShowChess()
        {
            Log.TestLine("The Chess:");
            for (int x = 0; x < chessPad_.GetGridStatusMap().Count; x++)
            {
                for (int y = 0; y < chessPad_.GetGridStatusMap()[0].Count; y++)
                {
                    Chess chess = chessPad_.GetChessMap()[x][y];
                    int posStatus = chessPad_.GetGridStatusMap()[x][y];
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
                    if (chess != null)
                    {
                        output = chess.GetChessProperty().Name;
                    }
                    Log.Test(Utils.FixLength(output, 15), textColor);
                }
                Log.Test("\n");
            }
        }

        Dictionary<int, Int2D> chessGridIndexMap_ = [];

        public override ChessPad InitChessPadStandard()
        {
            ChessPad chessPad = new ChessPad(3, 5);
            chessPad.InitStandard();
            int index = 0;
            for (int x = 0; x < chessPad.GetGridStatusMap().Count; x++)
            {
                for (int y = 0; y < chessPad.GetGridStatusMap()[0].Count; y++)
                {
                    chessGridIndexMap_.Add(index, new Int2D(x, y));
                    index++;
                }
            }
            return chessPad;
        }

        public void ShowInfo(string info, TextColor textColor = TextColor.NONE)
        {
            Log.TestLine(info, textColor);
            Log.TestLine("GameTurn: " + GameTurns + "\nInput ChessPad Index: " + stepList_[GameTurns].index + "\nCardCode: " + stepList_[GameTurns].cardCode, TextColor.BLACK);
            ShowChessGrid();
            ShowChess();
        }
        public override void RunGameTurns(int TimeEveryTurn)
        {
            Log.Clear();
            Log.TestLine("-----------------TEST START-----------------");
            player_.SetChessInHand(new List<Chess> { new Chess(Property.GetChessProperty("CardTest1")) });
            rival_.SetChessInHand(new List<Chess> { new Chess(Property.GetChessProperty("CardTest1")) });
            while (gameStatus_ == GameStatus.GAMING)
            {
                base.RunGameTurns(TimeEveryTurn);
            }
            Log.TestLine("-----------------TEST END-----------------");
        }
        public override void RivalTurn()
        {
            PlayerTurn();
        }
        bool Compare(List<List<int>> result, List<List<int>> expect)
        {
            if (!Utils.Compare(result, expect))
            {
                Log.TestLine("Campare Failed!", TextColor.RED, true);
                ShowInfo("Error Info:");
                ShowErrorDiff(expect, result);
                return false;
            }
            return true;
        }
        public override void PlayerTurn()
        {
            if (GameTurns >= stepList_.Count)
            {
                gameStatus_ = GameStatus.GAME_END;
                TestResult = true;
                GameTurns -= 1;
                ShowInfo("End status:");
                Log.TestLine("Success: Steps Done!", TextColor.GREEN, true);
                return;
            }
            int stepNum = GameTurns;
            Step step = stepList_[stepNum];
            Input input = new Input(chessGridIndexMap_[step.index], step.cardCode);
            if (!DoATurn((step.inputerType == InputerType.PLAYER) ? player_ : rival_, input))
            {
                gameStatus_ = GameStatus.GAME_END;
                TestResult = false;
                Log.TestLine("Error: DoATurn Failed!", TextColor.RED, true);
                Log.TestLine("Step " + stepNum + " Failed!", TextColor.RED, true);
            }
            else
            {
                List<List<int>> resultG = chessPad_.GetGridStatusMap();
                List<List<int>> resultC = chessPad_.GetCardLevelResult();
                List<List<int>> expectG = step.expectPad.GetExpectGridMap();
                List<List<int>> expectC = step.expectPad.GetExpectLevelMap();
                bool cardLevelError = false;
                bool gridLevelError;
                if ((gridLevelError = !Compare(resultG, expectG)) || (cardLevelError = !Compare(resultC, expectC)))
                {
                    gameStatus_ = GameStatus.GAME_END;
                    TestResult = false;
                    if (gridLevelError) Log.TestLine("Error: Compare Grid Level Failed!", TextColor.RED, true);
                    if (cardLevelError) Log.TestLine("Error: Compare Card Level Failed!", TextColor.RED, true);
                    Log.TestLine("Step " + stepNum + " Failed!", TextColor.RED, true);
                }
                else
                {
                    Log.TestLine("Step " + stepNum + " Success!", TextColor.GREEN, true);
                }
            }
            base.PlayerTurn();
        }

        private bool DoATurn(Gamer gamer, Input input)
        {
            if (gamer.AddInput(input))
            {
                gamer.CommitInput();
                return true;
            }
            else
            {
                gamer.RestoreSelect();
                ShowInfo("Rival AddInput Failed!", TextColor.RED);
            }
            return false;
        }
    }

    public class TestCase
    {
        protected TestSuite testSuite_;
        protected ChessPad initChessPad_;
        public bool Run()
        {
            initChessPad_ = InitChessPad();
            testSuite_ = new TestSuite(initChessPad_);
            InitSteps();
            return testSuite_.Run();
        }
        public virtual ChessPad InitChessPad()
        {
            ChessPad initChessPad = new ChessPad(3, 5);
            initChessPad.InitStandard();
            return initChessPad;
        }
        public virtual void AddStep(InputerType inputerType, int index, string cardCode, List<List<int>> expectPad1, List<List<int>> expectPad2 = null)
        {
            testSuite_.AddStep(new Step(
                inputerType,
                index, cardCode,
                new ExpectPad(
                    expectPad1,
                    expectPad2
                )
            ));
        }
        public virtual void InitSteps()
        {
        }
        protected readonly int O = 10;
        protected readonly int F1 = 1;
        protected readonly int F2 = 2;
        protected readonly int F3 = 3;
        protected readonly int E1 = 11;
        protected readonly int E2 = 12;
        protected readonly int E3 = 13;
        protected readonly int FF = 14;
        protected readonly int EE = 15;
    }
}