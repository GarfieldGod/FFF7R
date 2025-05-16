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

    public class ExpectPad : ChessPad
    {
        List<List<int>> expectGridMap_;
        public ExpectPad(List<List<int>> chessGridStatus) : base()
        {
            expectGridMap_ = chessGridStatus;
        }
        public List<List<int>> GetExpectGridMap()
        {
            return expectGridMap_;
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

        public static void ShowGridLevel(ChessPad chessPad)
        {
            for (int x = 0; x < chessPad.GetChessGridStatus().Count; x++)
            {
                for (int y = 0; y < chessPad.GetChessGridStatus()[0].Count; y++)
                {
                    Chess chess = chessPad.GetChessStatus()[x][y];
                    int posStatus = chessPad.GetChessGridStatus()[x][y];
                    TextColor textColor;
                    bool ifHightLight = true;
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
                        ifHightLight = false;
                    }

                    string output = (posStatus % 10).ToString();
                    if (chess != null)
                    {
                        output = chess.GetChessProperty().Name;
                    }
                    Log.Test(Utils.FixLength(output, 15), textColor, ifHightLight);
                }
                Log.Test("\n");
            }
        }

        public void ShowChessGrid()
        {
            Log.TestLine("The ChessGrid:", TextColor.BLUE, true);
            int index = 0;
            for (int x = 0; x < chessPad_.GetChessGridStatus().Count; x++)
            {
                for (int y = 0; y < chessPad_.GetChessGridStatus()[0].Count; y++)
                {
                    Chess chess = chessPad_.GetChessStatus()[x][y];
                    int posStatus = chessPad_.GetChessGridStatus()[x][y];
                    TextColor textColor;
                    bool ifHightLight = true;
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
                        ifHightLight = false;
                    }

                    string output = (posStatus % 10).ToString();
                    if (chess != null)
                    {
                        output = chess.GetChessProperty().Name;
                    }
                    Log.Test("(" + index.ToString("D2") + ")", TextColor.BLACK, false);
                    Log.Test(Utils.FixLength(output, 15), textColor, ifHightLight);
                    index++;
                }
                Log.Test("\n");
            }
        }

        public void ShowChess()
        {
            Log.TestLine("The Chess:", TextColor.BLUE, true);
            for (int x = 0; x < chessPad_.GetChessGridStatus().Count; x++)
            {
                for (int y = 0; y < chessPad_.GetChessGridStatus()[0].Count; y++)
                {
                    Chess chess = chessPad_.GetChessStatus()[x][y];
                    int posStatus = chessPad_.GetChessGridStatus()[x][y];
                    TextColor textColor;
                    bool ifHightLight = true;
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
                        ifHightLight = false;
                    }

                    string output = "null";
                    if (chess != null)
                    {
                        output = chess.GetChessProperty().Name;
                    }
                    Log.Test(Utils.FixLength(output, 15), textColor, ifHightLight);
                }
                Log.Test("\n");
            }
        }

        Dictionary<int, Int2D> chessGridIndexMap_ = [];

        public override ChessPad InitChessPadStandard()
        {
            ChessPad chessPad = new ChessPad();
            chessPad.InitStandard();
            int index = 0;
            for (int x = 0; x < chessPad.GetChessGridStatus().Count; x++)
            {
                for (int y = 0; y < chessPad.GetChessGridStatus()[0].Count; y++)
                {
                    chessGridIndexMap_.Add(index, new Int2D(x, y));
                    index++;
                }
            }
            return chessPad;
        }

        public void ShowInfo(string info, TextColor textColor = TextColor.NONE)
        {
            ShowChessGrid();
            ShowChess();
            Log.TestLine("GameTurn: " + GameTurns + "\nChessPad Index: " + stepList_[GameTurns].index + "\nCardCode: " + stepList_[GameTurns].cardCode, TextColor.YELLOW, true);
            Log.TestLine(info, textColor, true);
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
        public override void PlayerTurn()
        {
            if (GameTurns >= stepList_.Count)
            {
                TestResult = true;
                Log.TestLine("Success: GameTurns Done!", TextColor.GREEN);
                gameStatus_ = GameStatus.GAME_END;
            }
            else
            {
                Input input = new Input(chessGridIndexMap_[stepList_[GameTurns].index], stepList_[GameTurns].cardCode);
                if (!DoATurn((stepList_[GameTurns].inputerType == InputerType.PLAYER) ? player_ : rival_, input))
                {
                    TestResult = false;
                    Log.TestLine("Error: DoATurn Failed!", TextColor.RED);
                    ShowInfo("Step " + GameTurns + " Failed!", TextColor.RED);
                    gameStatus_ = GameStatus.GAME_END;
                }
                else
                {
                    if (!CompareChessPad(chessPad_, stepList_[GameTurns].expectPad))
                    {
                        TestResult = false;
                        Log.TestLine("Error: CompareChessPad Failed!", TextColor.RED);
                        ShowInfo("Step " + GameTurns + " Failed!", TextColor.RED);
                        gameStatus_ = GameStatus.GAME_END;
                    }
                    else
                    {
                        ShowInfo("Step " + GameTurns + " Success!", TextColor.GREEN);
                    }
                }
            }
            base.PlayerTurn();
        }

        private bool CompareChessPad(ChessPad nowPad, ExpectPad expectPad)
        {
            return Utils.Compare(nowPad.GetChessGridStatus(), expectPad.GetExpectGridMap());
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
            ChessPad initChessPad = new ChessPad();
            initChessPad.InitStandard();
            return initChessPad;
        }
        public virtual void AddStep(InputerType inputerType, int index, string cardCode, List<List<int>> expectPad1)
        {
            testSuite_.AddStep(new Step(
                inputerType,
                index, cardCode,
                new ExpectPad(
                    expectPad1
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