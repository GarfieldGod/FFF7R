using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FFF7R;

public static class DeckRules
{
    public const int DECK_COUNT = 6;
    public const int DECK_SIZE  = 15;

    private const int ADVANCE_MAX_COUNT = 1;
    private const int NORMAL_MAX_COUNT = 2;

    public static int MaxCount(this string cardCode)
    {
        if (!App.VerifyCadeCode(cardCode)) return 0;
        return App.Cards.Get(cardCode).MaxCount();
    }

    public static int MaxCount(this CardDefView def)
    {
        return def.Type == CardType.Advance ? ADVANCE_MAX_COUNT : NORMAL_MAX_COUNT;
    }
}
