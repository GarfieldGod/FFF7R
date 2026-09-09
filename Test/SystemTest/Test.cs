using Xunit;

namespace Test
{
    public class SystemTest
    {
        [Fact]
        public static void Run()
        {
            TestSuite test = new TestSuite();
            test.Add(new TestCase1());
            Assert.True(test.Run(), "系统测试失败");
        }
    }
}