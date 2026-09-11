namespace FFF7RCore.Test {
    public class TestGame : Game
    {
        public bool TestResult => testResult_;

        private readonly List<Step> stepList_;
        private bool testResult_ = false;
        private Dictionary<int, Int2D> chessGridIndexMap_ = [];

        public TestGame(List<Step> steps, GameConfig GameConfig) : base(GameConfig)
        {
            stepList_ = steps;
        }

        public ChessPad InitChessPadStandard()
        {
            ChessPad chessPad = new ChessPad(3, 5);
            chessPad.InitStandard();
            int index = 0;
            for (int x = 0; x < chessPad.StatusMap.Count; x++)
            {
                for (int y = 0; y < chessPad.StatusMap[0].Count; y++)
                {
                    chessGridIndexMap_.Add(index, new Int2D(x, y));
                    index++;
                }
            }
            return chessPad;
        }

        // public void RunGameTurns(int TimeEveryTurn)
        // {
        //     Log.Clear();
        //     Log.TestLine("-----------------TEST START-----------------");
        //     player_.ChessInHand = new List<Chess> { new Chess(Property.GetChessProperty("CardTest1")) };
        //     rival_.ChessInHand = new List<Chess> { new Chess(Property.GetChessProperty("CardTest1")) };
        //     while (gameStatus_ == GameStatus.GAMING)
        //     {
        //         base.RunGameTurns(TimeEveryTurn);
        //     }
        //     Log.TestLine("-----------------TEST END-----------------");
        // }
        // public void RivalTurn()
        // {
        //     PlayerTurn();
        // }

        // bool Compare(List<List<int>> result, List<List<int>> expect)
        // {
        //     if (!Utils.Compare(result, expect))
        //     {
        //         Log.TestLine("Campare Failed!", TextColor.RED, true);
        //         ShowInfo("Error Info:");
        //         TestUtils.ShowErrorDiff(expect, result);
        //         return false;
        //     }
        //     return true;
        // }

        // public void PlayerTurn()
        // {
        //     if (Turns >= stepList_.Count)
        //     {
        //         gameStatus_ = GameStatus.GAME_END;
        //         testResult_ = true;
        //         ShowInfo("End status:");
        //         Log.TestLine("Success: Steps Done!", TextColor.GREEN, true);
        //         return;
        //     }
        //     int stepNum = Turns - 1;
        //     Step step = stepList_[stepNum];
        //     Input input = new Input(chessGridIndexMap_[step.index], step.cardCode);
        //     if (!DoATurn((step.playerType == PlayerType.PLAYER) ? player_ : rival_, input))
        //     {
        //         gameStatus_ = GameStatus.GAME_END;
        //         testResult_ = false;
        //         Log.TestLine("Error: DoATurn Failed!", TextColor.RED, true);
        //         Log.TestLine("Step " + stepNum + " Failed!", TextColor.RED, true);
        //     }
        //     else
        //     {
        //         List<List<int>> resultG = chessPad_.StatusMap;
        //         List<List<int>> resultC = chessPad_.GetCardLevelResult();
        //         List<List<int>> expectG = step.expectPad.GetExpectGridMap();
        //         List<List<int>> expectC = step.expectPad.GetExpectLevelMap();
        //         bool cardLevelError = false;
        //         bool gridLevelError;
        //         if ((gridLevelError = !Compare(resultG, expectG)) || (cardLevelError = !Compare(resultC, expectC)))
        //         {
        //             gameStatus_ = GameStatus.GAME_END;
        //             testResult_ = false;
        //             if (gridLevelError) Log.TestLine("Error: Compare Grid Level Failed!", TextColor.RED, true);
        //             if (cardLevelError) Log.TestLine("Error: Compare Card Level Failed!", TextColor.RED, true);
        //             Log.TestLine("Step " + stepNum + " Failed!", TextColor.RED, true);
        //         }
        //         else
        //         {
        //             Log.TestLine("Step " + stepNum + " Success!", TextColor.GREEN, true);
        //         }
        //     }
        //     base.PlayerTurn();
        // }

        // private bool DoATurn(GamePlayer gamer, Input input)
        // {
        //     if (gamer.AddInput(input))
        //     {
        //         gamer.CommitInput();
        //         return true;
        //     }
        //     else
        //     {
        //         gamer.RestoreSelect();
        //         ShowInfo("Rival AddInput Failed!", TextColor.RED);
        //     }
        //     return false;
        // }
    }
}