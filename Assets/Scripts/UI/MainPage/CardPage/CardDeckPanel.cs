using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardDeckPanel : MonoBehaviour
{
    [SerializeField] private NavUI cardDeckNav;
    [SerializeField] private CardUI[] cardUI;

    public Action<string> OnCardSelected;
    // Start is called before the first frame update
    void Awake()
    {
        cardDeckNav.OnNavButtonSelected += ShowDeck;

        for (int i = 0; i < cardUI.Length; i++)
        {
            cardUI[i].Selected += SelectCard;
        }
    }

    void OnDestroy()
    {
        cardDeckNav.OnNavButtonSelected -= ShowDeck;
        for (int i = 0; i < cardUI.Length; i++)
            cardUI[i].Selected -= SelectCard;
    }

    void SelectCard(string cardCode) => OnCardSelected?.Invoke(cardCode);

    void ShowDeck(int index)
    {
        var cardList = App.Save.CardDeck(index);
        for (int i = 0; i < cardUI.Length; i++)
        {
            cardUI[i].Bind(cardList[i]);
        }
    }
}
