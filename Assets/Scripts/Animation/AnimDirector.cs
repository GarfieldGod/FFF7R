using System;
using System.Collections.Generic;
using FFF7R;

public class AnimDirector
{
    public static IAnimStep EventToAnimStep(CardBoard board, GameEvent e, Action onBegin)
    {
        switch (e)
        {
            case ChessPlacedEvent p:
                return new CardPlacedStep(board[p.Pos], p.CardCode, p.Owner, onBegin);
            case ChessBuffedEvent b:
            case ChessDeadEvent d:
            case BuffsExpiredEvent x:
                return new ActionStep(onBegin);
            case GridLevelsChangedEvent g:
                var dict = new Dictionary<BoardGrid, CellDelta>();
                foreach (var c in g.Cells)
                {
                    dict[board[c.Pos]] = c;
                }
                return new CellUpgradeStep(dict, onBegin);
            case TurnEndedEvent t:
                // if (!t.IsPass)                                  // 跳过的回合不播
                    // _queue.Enqueue(new TurnStep(t.NextPlayer, t.Turns));
                return null;
            case GameEndedEvent end:
                // _queue.Enqueue(new EndStep(end.Winner, end.Turns));
                return null;
            default:
                return null;
        }
    }
}