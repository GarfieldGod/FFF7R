using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DeckInfoPanel : MonoBehaviour
{
    private const string DECK_NAME = "牌组No.";
    private static readonly Color DECK_NUM_COLOR = new Color(191/255f, 149/255f, 88/255f, 1f);
    [SerializeField] private TMP_Text deckName;
    [SerializeField] private TMP_Text deckCardNum;

    public void SetDeckInfo(int index, int count)
    {
        deckName.text = DECK_NAME + (index + 1).ToString();
        deckCardNum.text = count.ToString();

        if (count != DeckRules.DECK_SIZE) deckCardNum.color = Color.red;
        else deckCardNum.color = DECK_NUM_COLOR;
    }

    public void SetDeckInfo(DeckSlot deck) => SetDeckInfo(deck.index, deck.Count);
}
