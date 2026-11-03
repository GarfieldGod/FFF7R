namespace FFF7R.Adapter
{
    using System.Collections.Generic;

    public class AiBrain: IAiBrain
    {
        public bool TryDecide(IGameQuery query, PlayerSeat seat, out AiDecision decision)
        {
            decision = new AiDecision(-1, new Int2(-1, -1));

            HandSnapshot hand = query.Hand(seat);
            BoardSnapshot board = query.Board;

            bool found = false;
            int best = int.MinValue;

            foreach(var cell in board.Cells) {
                if (cell.State.Owner() != seat) continue;

                for (int i = 0; i < hand.Count; i++)
                {
                    if (!query.CanPlace(seat, i, cell.Pos, out _)) continue;

                    BoardSnapshot preview = query.Preview(seat, i, cell.Pos);
                    if (preview == null) continue;

                    int gain = seat == PlayerSeat.Player
                        ? preview.PlayerScore - board.PlayerScore
                        : preview.RivalScore - board.RivalScore;

                    if (gain > best || !found)
                    {
                        best = gain;
                        decision = new AiDecision(i, cell.Pos);
                        found = true;
                    }
                }
            }

            return found;
        }
    }
}