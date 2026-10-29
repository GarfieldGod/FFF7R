namespace FFF7RCore.Test {
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public abstract class TestCase
    {
        public string Description;
        public List<string> CoveredCards => steps_.Select(x => x.input.chess.CardCode).ToList();
        public Dictionary<Step, bool> StepsResult => stepsRet_;

        protected bool IgnoreTurnLimit = false;
        protected bool StopTestCaseWhenFailed = true;
        protected bool NoLog = false;

        private TestGame testGame_;
        private List<Step> steps_ = new List<Step>();
        private int stepIndex_ = 0;
        private bool result_;
        private Dictionary<Step, bool> stepsRet_ = new Dictionary<Step, bool>();

        public TestCase()
        {
            ConfigureOptions();
            InitSteps();

            var gameConfig = InitGameConfig();
            testGame_ = new TestGame(steps_, gameConfig);
        }

        /// <summary>
        /// 跑本用例。默认实现：按 <see cref="InitSteps"/> 声明的剧本逐条出牌，每步比对棋盘。
        /// 想完全不按剧本跑的用例（例如 E2E 套）可以覆写本方法——但**必须**用
        /// <see cref="RecordStepResult"/> 登记结果，否则 <see cref="TestSuite"/> 的 Passed / Failed 统计会漏。
        /// </summary>
        public virtual bool Run()
        {
            if (NoLog) Log.PrintToConsole = false;

            if (IgnoreTurnLimit)
            {
                testGame_.IgnoreTurnLimit = true;
            }
            testGame_.Init(true);
            testGame_.Start(steps_[0].input.playerType);

            result_ = true;
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
                    stepsRet_[step] = false;
                    result_ = false;
                    if (StopTestCaseWhenFailed) return false;
                } else {
                    stepsRet_[step] = true;
                }

                stepIndex_++;
            }

            Log.PrintToConsole = true;
            return result_;
        }

        /// <summary>
        /// 给覆写 <see cref="Run"/> 的子类登记一条结果，下标对应 <see cref="AddStep"/> 的调用顺序。
        /// <para>
        /// 为什么需要它：<see cref="TestSuite"/> 的 Passed / Failed 不是看 <see cref="Run"/> 的返回值，
        /// 而是看 <see cref="StepsResult"/> 里每步的真假。覆写 Run 的用例如果一条都不登记，
        /// 它失败时会被统计成 "Failed: 0"，只有最终 Test Result 变红——数字自相矛盾。
        /// </para>
        /// </summary>
        protected void RecordStepResult(int stepIndex, bool ok)
        {
            if (stepIndex < 0 || stepIndex >= steps_.Count) return;
            stepsRet_[steps_[stepIndex]] = ok;
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

        public virtual void AddStep(PlayerType playerType, string cardCode, Int2D inputPos, List<List<int>> expectPad1, List<List<int>> expectPad2 = null, bool skip = false)
        {
            Input input = new Input(inputPos, cardCode, playerType);
            steps_.Add(new Step(steps_.Count, input,
                new ExpectPad(
                    expectPad1,
                    expectPad2
                )
                ,skip
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
            if (step.skip) return ret;
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
            Log.TestLine("Compare " + name + " Success!", TextColor.GREEN, true);
            return true;
        }
    }
}