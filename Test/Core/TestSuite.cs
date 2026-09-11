namespace FFF7RCore.Test {
    using System.Text;

    public class TestSuite
    {
        public bool InterruptedByFailure { get; set; }
        private List<TestCase> caseList_;
        private Dictionary<TestCase, bool> testCaseResultList_;
        private HashSet<string> uncoveredCardList_;
        private bool testResult_ = true;
        private int coveredCardsNum_ = 0;

        public TestSuite()
        {
            caseList_ = new List<TestCase> {};
            testCaseResultList_ = new Dictionary<TestCase, bool>();
            Property.LoadChessProperties();
            uncoveredCardList_ = new HashSet<string>(Property.GetAllCardCode());
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
                    testCaseResultList_[testCase] = false;
                    testResult_ = false;
                    if (InterruptedByFailure) break;
                } else {
                    testCaseResultList_[testCase] = true;
                    foreach (var cardCode in testCase.CoveredCards)
                    {
                        if (uncoveredCardList_.Contains(cardCode))
                        {
                            coveredCardsNum_++;
                            uncoveredCardList_.Remove(cardCode);
                        }
                    }
                }
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
            
            int passed = testCaseResultList_.Count(x => x.Value);
            int failed = testCaseResultList_.Count(x => !x.Value);
            Log.TestLine($"Passed: {passed}", TextColor.GREEN);
            Log.TestLine($"Failed: {failed}", failed == 0 ? TextColor.BLACK : TextColor.RED);
            if (failed != 0)
            {
                Log.TestLine("\nFailed Cases: ", TextColor.RED);
                foreach (var testCase in testCaseResultList_.Where(x => !x.Value))
                {
                    Log.Test($"\t[{testCase.Key} step {testCase.Key.FailedStep}]", TextColor.RED);
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