namespace FFF7RCore.Test {
    public abstract class TestCase
    {
        public List<string> CoveredCards => steps_.Select(x => x.input.chess.CardCode).ToList();
        public int FailedStep => result_ ? -1 : stepIndex_;
        public string Description;

        protected TestGame testGame_;
        protected List<Step> steps_ = new List<Step>();
        protected bool IgnoreTurnLimit = false;
        protected int stepIndex_ = 0;
        protected bool result_;

        public TestCase()
        {
            ConfigureOptions();
            InitSteps();

            var gameConfig = InitGameConfig();
            testGame_ = new TestGame(steps_, gameConfig);
        }

        public bool Run()
        {
            if (IgnoreTurnLimit)
            {
                testGame_.IgnoreTurnLimit = true;
            }
            testGame_.Init(true);
            testGame_.Start(steps_[0].input.playerType);

            while (testGame_.Status != GameStatus.GAME_OVER && stepIndex_ < steps_.Count)
            {
                Log.TestLine($"\n----------------[{this} step {stepIndex_}]----------------");
                var step = steps_[stepIndex_];
                Log.TestLine($"\n------------DebugInfo------------", TextColor.PURPLE);
                bool ret = testGame_.Input(step.input);

                TestUtils.ShowStepInfo(step, stepIndex_);
                Log.TestLine("Input ret: " + ret);
                TestUtils.ShowGameInfo(testGame_);

                if (!CheckResult(step))
                {
                    result_ = false;
                    return false;
                }

                stepIndex_++;
            }
            result_ = true;
            return true;
        }

        public virtual GameConfig InitGameConfig()
        {
            var initPad = InitChessPad();
            var playerChessPool = new List<String>{};
            var rivalChessPool = new List<String>{};
            return new GameConfig(playerChessPool, rivalChessPool, initPad);
        }

        public virtual ChessPad InitChessPad()
        {
            ChessPad initChessPad = new ChessPad(3, 5);
            initChessPad.InitStandard();
            return initChessPad;
        }

        public virtual void AddStep(PlayerType playerType, string cardCode, Int2D inputPos, List<List<int>> expectPad1, List<List<int>> expectPad2 = null)
        {
            Input input = new Input(inputPos, cardCode, playerType);
            steps_.Add(new Step(
                input,
                new ExpectPad(
                    expectPad1,
                    expectPad2
                )
            ));
        }

        public virtual void ConfigureOptions() { Description = "No description."; }
        public abstract void InitSteps();
        protected readonly int O = 10;
        protected readonly int F1 = 1;
        protected readonly int F2 = 2;
        protected readonly int F3 = 3;
        protected readonly int E1 = 11;
        protected readonly int E2 = 12;
        protected readonly int E3 = 13;
        protected readonly int FF = 14;
        protected readonly int EE = 15;

        bool CheckResult(Step step) {
            bool ret = true;
            if (step.expectPad.PosStatus != null && 
                !Compare(testGame_.ChessPad.StatusMap, step.expectPad.PosStatus, "PosStatus")) 
                ret = false;
            if (step.expectPad.LevelStatus != null && 
                !Compare(testGame_.ChessPad.CardLevelMap, step.expectPad.LevelStatus, "CardLevel")) 
                ret = false;
            return ret;
        }

        bool Compare(List<List<int>> result, List<List<int>> expect, string name = "")
        {
            if (!Utils.Compare(result, expect))
            {
                Log.TestLine("Compare " + name + " Failed!", TextColor.RED, true);
                TestUtils.ShowErrorDiff(expect, result);
                return false;
            }

            return true;
        }
    }
}