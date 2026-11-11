using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FFF7R;

public sealed class LocalPlayerActionSource: ISeatActionSource
{
    public CardSelector cardSelector;
    private bool _passRequested = false;

    public LocalPlayerActionSource(CardSelector cardSelector)
    {
        this.cardSelector = cardSelector;
    }

    public void RequestPass() => _passRequested = true;

    public void OnTurnStart(PlayerSeat seat) => cardSelector.TurnStart();

    public void OnTurnEnd(PlayerSeat seat) => cardSelector.TurnEnd();

    public bool TryTake(IGameQuery query, PlayerSeat seat, out SeatAction action)
    {
        if (_passRequested)                       // ← 优先级最高，先于选牌
        {
            _passRequested = false;               // 读即清
            action = SeatAction.Pass();
            return true;
        }

        cardSelector.TurnTick();
        action = cardSelector.TakeAction();
        return action.ActionKind != SeatAction.Kind.None;
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
}
