namespace FFF7RCore
{
    public readonly struct ScoreLine
    {
        public readonly int Player;
        public readonly int Rival;
        public ScoreLine(int player, int rival)
        {
            Player = player;
            Rival = rival;
        }
    }

    public class Score
    {
        private List<ScoreLine> score_ = new List<ScoreLine>();
        public int Count => score_.Count;
        public int PlayerScore => score_.Sum(x => x.Player);
        public int RivalScore => score_.Sum(x => x.Rival);
        public PlayerType Winner => PlayerScore > RivalScore ? PlayerType.PLAYER : PlayerType.RIVAL;

        public void AddLine(int playerScore, int rivalScore)
        {
            score_.Add(new ScoreLine(playerScore, rivalScore));
        }

        public int GetLineScore(int lineIndex, PlayerType player)
        {
            if (lineIndex < 0 || lineIndex >= score_.Count) return -1;

            return player == PlayerType.PLAYER ? score_[lineIndex].Player : score_[lineIndex].Rival;
        }
    }
}