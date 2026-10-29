namespace FFF7RCore.Test {
    using System.Collections.Generic;
    public class TestGame : Game
    {
        public bool TestResult => testResult_;

        private readonly List<Step> stepList_;
        private bool testResult_ = false;
        private Dictionary<int, Int2D> chessGridIndexMap_ = new Dictionary<int, Int2D>();

        public TestGame(List<Step> steps, GameConfig GameConfig) : base(GameConfig)
        {
            stepList_ = steps;
        }

        /// <summary>
        /// 脚本化用例不是真实对局：它没有手牌（每一步直接指定卡号），棋盘也是固定摆好的。
        /// 若套用真实规则的终止/pass 判定，第一步之后就会被判定结束，
        /// 用例因此覆盖不到后续步骤。这里关掉它。
        /// 真实对局的终止条件由 <c>--scan/--rate</c>（走 <c>GameSession</c>）覆盖。
        /// </summary>
        protected override bool UseRealMatchRules => false;

        public ChessPad InitChessPadStandard()
        {
            ChessPad chessPad = new ChessPad(3, 5);
            chessPad.InitStandard();
            int index = 0;
            for (int x = 0; x < chessPad.StatusMap.Count; x++)
            {
                for (int y = 0; y < chessPad.StatusMap[0].Count; y++)
                {
                    chessGridIndexMap_.Add(index, new Int2D(x, y));
                    index++;
                }
            }
            return chessPad;
        }
    }
}