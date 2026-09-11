namespace Test
{
    public class TestSuite
    {
        private List<TestCase> caseList_;
        public TestSuite()
        {
            caseList_ = new List<TestCase> {};
            Property.LoadChessProperties();
        }

        public void Add(TestCase testCase)
        {
            caseList_.Add(testCase);
        }

        public bool Run()
        {
            bool ret = true;
            foreach (TestCase testCase in caseList_)
            {
                if (!testCase.Run()) {
                    ret = false;
                    return ret;
                }
            }
            return ret;
        }
    }
}