namespace FFF7RCore.Test {
    using System.Text;

    public class TestSuite : IDisposable
    {
        public string LogOutputDir = "output";
        public bool InterruptedByFailure { get; set; }
        private List<TestCase> caseList_;
        private Dictionary<TestCase, Dictionary<Step, bool>> testCaseStepsResultDict_ = new Dictionary<TestCase, Dictionary<Step, bool>>();
        private HashSet<string> uncoveredCardList_;
        private bool testResult_ = true;
        private int coveredCardsNum_ = 0;
        private bool disposed_ = false;

        public TestSuite()
        {
            Log.SaveStart();
            caseList_ = new List<TestCase> {};
            Property.LoadChessProperties();
            uncoveredCardList_ = new HashSet<string>(Property.GetAllCardCode());
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed_) return;
            if (disposing)
            {
                string fileName = Path.Combine(LogOutputDir, 
                    "TestResult" + DateTime.Now.ToString("_HH_mm_ss_fff") + ".log");
                Log.SaveEnd(fileName);
            }
            disposed_ = true;
        }

        ~TestSuite()
        {
            Dispose(false);
        }

        public void Add(TestCase testCase)
        {
            caseList_.Add(testCase);
        }

        public bool Run()
        {
            foreach (TestCase testCase in caseList_)
            {
                if (!testCase.Run()) {
                    testResult_ = false;
                    if (InterruptedByFailure) break;
                } else {
                    foreach (var cardCode in testCase.CoveredCards)
                    {
                        if (uncoveredCardList_.Contains(cardCode))
                        {
                            coveredCardsNum_++;
                            uncoveredCardList_.Remove(cardCode);
                        }
                    }
                }
                testCaseStepsResultDict_[testCase] = testCase.StepsResult;
            }

            PrintResult();
            return testResult_;
        }

        public void PrintResult()
        {
            Log.TestLine("\n-----------------Test Result-----------------\n", TextColor.BLUE);
            Log.TestLine($"InterruptedByFailure: {InterruptedByFailure}\n", TextColor.YELLOW);
            Log.TestLine($"Cards Covered: {coveredCardsNum_}\tUncovered: {uncoveredCardList_.Count}", TextColor.YELLOW);
            if (uncoveredCardList_.Count > 0)
            {
                Log.TestLine($"Uncovered Cards: ");
                // Log.TestLine($"{string.Join(", ", uncoveredCardList_)}", TextColor.BLACK);
                Log.TestLine($"{JoinWithNewline(uncoveredCardList_, 10)}", TextColor.BLACK);
            }
            Log.TestLine($"\nTotal Count: {caseList_.Count}", TextColor.YELLOW);

            int passed = testCaseStepsResultDict_.Where(kvp => kvp.Value.All(s => s.Value)).Select(kvp => kvp.Key).ToList().Count;
            int failed = testCaseStepsResultDict_.Count - passed;
            Log.TestLine($"Passed: {passed}", TextColor.GREEN);
            Log.TestLine($"Failed: {failed}", failed == 0 ? TextColor.BLACK : TextColor.RED);
            if (failed != 0)
            {
                Log.TestLine("\nFailed Cases: ", TextColor.RED);
                foreach (var caseNameStepsResultPair in testCaseStepsResultDict_)
                {
                    foreach (var stepRetPair in caseNameStepsResultPair.Value.Where(x => !x.Value))
                        Log.Test($"\t[{caseNameStepsResultPair.Key} step {stepRetPair.Key.index}]", TextColor.RED);
                }
            }
            Log.TestLine($"\nTest Result: {testResult_}", testResult_ ? TextColor.GREEN : TextColor.RED);
            Log.TestLine("\n-----------------Test End-----------------", TextColor.BLUE);
        }

        string JoinWithNewline<T>(IEnumerable<T> list, int countPerLine)
        {
            var source = list.ToList();
            var sb = new StringBuilder();
            for (int i = 0; i < source.Count; i++)
            {
                sb.Append(source[i]);
                // 不是最后一个元素
                if (i != source.Count - 1)
                {
                    sb.Append(", ");
                    // 每 countPerLine 个之后换行
                    if ((i + 1) % countPerLine == 0)
                    {
                        sb.Append("\n");
                    }
                }
            }
            return sb.ToString();
        }
    }
}