namespace FFF7RCore.Test {
    using Xunit;
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public class SystemTest
    {
        [Fact]
        public static void Run()
        {
            Assert.True(RunTest(), "系统测试失败");
        }

        public static bool RunTest()
        {
            Assembly asm = Assembly.GetExecutingAssembly();

            // 查找所有继承自 TestCase 的类
            List<Type> testCaseTypes = asm.GetTypes()
                .Where(t => 
                    t.IsClass                // 是类
                    && !t.IsAbstract         // 排除抽象类，不能实例化
                    && typeof(TestCase).IsAssignableFrom(t)) // t 继承/实现 TestCase
                .ToList();

            // 批量实例化
            TestSuite test = new TestSuite();
            foreach (Type type in testCaseTypes)
            {
                // 调用无参构造函数实例化
                TestCase instance = Activator.CreateInstance(type) as TestCase;
                if (instance != null)
                {
                    test.Add(instance);
                    Log.TestLine($"实例化TestCase: {type.Name}", TextColor.BLACK);
                }
            }

            return test.Run();
        }
    }
}