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
                return new CardPlacedStep(board, p.Pos, p.CardCode, p.Owner, onBegin);
            case ChessBuffedEvent b:
                return new CardUpgradeStep(board, b.Pos, b.Value, onBegin);
            case GridLevelsChangedEvent g:
                return new CellUpgradeStep(board, g.Cells, onBegin);
            case ChessDeadEvent d:
            case BuffsExpiredEvent x:
                return new ActionStep(onBegin);
            case TurnEndedEvent t:
            case GameEndedEvent end:
            default:
                return null;
        }
    }
}