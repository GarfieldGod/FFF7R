using Xunit;

namespace Test
{
    public class SystemTest
    {
        [Fact]
        public static void Run()
        {
            Test.TestCase1 testCase1 = new Test.TestCase1();
            Assert.True(testCase1.Run(), "系统测试失败");
        }
    }
}