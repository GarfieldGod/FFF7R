using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FFF7R;

public sealed class LocalPlayerActionSource: ISeatActionSource
{
    public CardSelector cardSelector;
    public LocalPlayerActionSource(CardSelector cardSelector)
    {
        this.cardSelector = cardSelector;
    }

    public void OnResult(CommandResult result)
    {
        if (result.Ok)
        {
            cardSelector.OnInputSuccess();
        } else {
            Debug.Log($"无法落子：{result.Reason}");
        }
    }

    public bool TryTake(IGameQuery query, PlayerSeat seat, out SeatAction action)
    {
        cardSelector.TickSelect();
        action = cardSelector.TakeAction();
        return action.ActionKind != SeatAction.Kind.None;
    }
}
