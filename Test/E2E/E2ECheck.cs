namespace FFF7RCore.Test.E2E
{
    using System.Collections.Generic;

    /// <summary>
    /// 极简断言收集器。
    ///
    /// 刻意不抛异常：一次跑完能把所有失败点都列出来，而不是死在第一条。
    /// 刻意不引 nunit：内核要同时编进 Unity（netstandard2.1）和 dotnet（net8.0），
    /// 测试框架不该成为内核的依赖。
    /// </summary>
    public sealed class E2ECheck
    {
        private readonly List<string> failures_ = new List<string>();
        private readonly List<string> notes_ = new List<string>();

        /// <summary>已执行的断言条数。</summary>
        public int Assertions { get; private set; }

        /// <summary>失败条数。</summary>
        public int Failed => failures_.Count;

        public bool Ok => failures_.Count == 0;

        public IReadOnlyList<string> Failures => failures_;

        /// <summary>各子套的统计摘要（不断言，只用于输出）。</summary>
        public IReadOnlyList<string> Notes => notes_;

        public void That(bool condition, string context, string message)
        {
            Assertions++;
            if (!condition) failures_.Add(context + ": " + message);
        }

        public void Fail(string context, string message)
        {
            Assertions++;
            failures_.Add(context + ": " + message);
        }

        public void Note(string line)
        {
            notes_.Add(line);
        }
    }
}
