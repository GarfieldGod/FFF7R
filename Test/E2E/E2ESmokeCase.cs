namespace FFF7RCore.Test.E2E
{
    /// <summary>
    /// 把 <see cref="E2ERunner"/> 挂进既有 <see cref="SystemTest"/> 自动发现的薄壳。
    ///
    /// <para>
    /// <see cref="SystemTest"/> 用反射扫程序集里所有 <see cref="TestCase"/> 子类，
    /// 所以这个类只做两件事：被扫到、把 E2E 的成败登记回引擎。
    /// 真正的断言全部在 <see cref="E2ERunner"/> 里，通过 <see cref="IGameSession"/> 驱动。
    /// </para>
    ///
    /// <para>
    /// 覆写 <see cref="TestCase.Run"/> 后不再走「按剧本出牌」那条路，
    /// 但 <see cref="TestCase.AddStep"/> 仍要登记一条占位步骤：
    /// <see cref="TestSuite"/> 的 Passed / Failed 是按 <see cref="TestCase.StepsResult"/> 统计的，
    /// 没有步骤就没有格子承载结果。占位卡用 TestCards.json 的 <c>PosFriend4Up1</c>，
    /// 它已被 TestCase1 覆盖，不会污染 Cards Covered 统计。
    /// </para>
    /// </summary>
    public sealed class E2ESmokeCase : TestCase
    {
        private const string PlaceholderCard = "PosFriend4Up1";

        public override void ConfigureOptions()
        {
            Description = "E2E：200 局不变式 + 40 局冒烟 + 10 对种子复现 + 拒绝原因";
            StopTestCaseWhenFailed = true;
            // E2E 自己不做 per-step 比对，占位步骤也不会被真正执行
            NoLog = false;
        }

        public override void InitSteps()
        {
            // 只为给 TestSuite 留一个承载结果的槽位；本用例覆写了 Run，这步不会被出牌引擎执行。
            AddStep(
                PlayerType.PLAYER,
                PlaceholderCard,
                new Int2D(0, 0),
                null,
                null,
                false);
        }

        public override bool Run()
        {
            E2ECheck check = new E2ECheck();

            // 跑完会在 Log 里打一张摘要（E2ERunner.Print）
            E2ERunner.RunAll(check);

            bool passed = check.Ok;

            Description = passed
                ? "E2E 全绿（" + check.Assertions + " 条断言）"
                : "E2E 失败 " + check.Failed + " / " + check.Assertions + " 条断言";

            RecordStepResult(0, passed);
            return passed;
        }
    }
}
