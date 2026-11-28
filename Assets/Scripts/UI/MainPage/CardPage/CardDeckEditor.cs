using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CardDeckEditor : MonoBehaviour
{
    [SerializeField] CardSlotsView CardList;
    [SerializeField] CardSlotsView DeckList;
    [SerializeField] DeckInfoPanel deckInfoPanel;

    DeckSlot _tmpDeckSlot;

    public IReadOnlyList<string> DeckCodes => _tmpDeckSlot.cards;

    public IReadOnlyList<string> OwnedCodes => App.Save.OwnedCards;

    public IReadOnlyList<string> AvailableCodes
    {
        get {
            var list = new List<string>(OwnedCodes);
            foreach (var c in _tmpDeckSlot.cards) list.Remove(c);
            return list;
        }
    }

    void Awake()
    {
        CardList.OnCardClick += AddCardToDeck;
        DeckList.OnCardClick += RemoveCardFromDeck;
    }

    void OnDestroy()
    {
        CardList.OnCardClick -= AddCardToDeck;
        DeckList.OnCardClick -= RemoveCardFromDeck;
    }

    public void EditDeck(int index)
    {
        var cardCodes = App.Save.CardDeck(index);

        _tmpDeckSlot = new DeckSlot(index);
        foreach (var code in cardCodes) _tmpDeckSlot.Add(code);

        Flush();
        CardList.ScrollToTop();
    }

    public void Flush()
    {
        deckInfoPanel.SetDeckInfo(_tmpDeckSlot);
        CardList.ShowCards(OwnedCodes, AvailableCodes);
        DeckList.ShowCards(DeckCodes, DeckCodes);
    }

    public bool SaveDeck()
    {
        if (_tmpDeckSlot.cards.Count != DeckRules.DECK_SIZE) return false;
        return App.Save.SetDeck(_tmpDeckSlot.index, _tmpDeckSlot.cards);
    }

    void AddCardToDeck(string code)
    {
        if (_tmpDeckSlot.cards.Count >= DeckRules.DECK_SIZE) return;
        if (!AvailableCodes.Contains(code)) return;

        _tmpDeckSlot.Add(code);
        Flush();
    }

    void RemoveCardFromDeck(string code)
    {
        _tmpDeckSlot.Remove(code);
        Flush();
    }
}