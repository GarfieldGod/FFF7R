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