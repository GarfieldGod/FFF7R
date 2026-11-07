using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[Serializable]
public sealed class CardHover
{
    public Card Current;

    /// <summary>返回本帧被指到的卡；null 表示没有。单出口，不会残留上一帧的值。</summary>
    public void Tick(IReadOnlyList<Card> cards, GameObject go, Camera cam, bool selecting)
    {
        if (go == null || selecting)
        {
            Current = null;
            return;
        }

        if (go.CompareTag("CardInHand") && go.TryGetComponent<Card>(out var c) && cards.Contains(c)) {
            Current = c;
        }
        else 
            Current = null;
    }
}