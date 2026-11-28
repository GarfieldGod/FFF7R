using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

[Serializable]
public class SaveData
{
    public int version = 1;

    public string username;
    public int userLevel;

    public PlayerCardData cardData = new PlayerCardData();

    public List<string> clearedLevels = new List<string>();
    public string currentRegionId;
}

[Serializable]
public class PlayerCardData
{
    public List<string>   owned = new List<string>();
    public List<DeckSlot> decks = new List<DeckSlot>();

    public PlayerCardData()
    {
        for (int i = 0; i < DeckRules.DECK_COUNT; i++)
            decks.Add(new DeckSlot(i + 1));
    }

    public IReadOnlyList<string> GetDeck(int index)
    {
        if (index < 0 || index >= DeckRules.DECK_COUNT) return null;
        return decks[index].AsReadOnly();
    }

    public bool SetDeck(int index, List<string> cards)
    {
        if (index < 0 || index >= DeckRules.DECK_COUNT) return false;

        decks[index].Set(cards);
        return true;
    }

    public IReadOnlyList<string> OwnedCards => owned.AsReadOnly();
    public bool ObtainCard(string cardCode)
    {
        if (!App.VerifyCadeCode(cardCode)) return false;
        if (owned.Contains(cardCode) && owned.Count(s => s == cardCode) >= DeckRules.MaxCount(cardCode)) return false;

        owned.Add(cardCode);
        return true;
    }
}

[Serializable]
public class DeckSlot
{
    public int index;
    public string name = string.Empty;
    public List<string> cards = new List<string>();
    public DeckSlot(int index)
    {
        this.index = index;
    }

    public void Add(string cardCode)
    {
        if (App.VerifyCadeCode(cardCode)) cards.Add(cardCode);
    }

    public void Remove(string cardCode) => cards.Remove(cardCode);

    public void Set(List<string> newList)
    {
        cards.Clear();
        foreach (var card in newList) cards.Add(card);
    }

    public ReadOnlyCollection<string> AsReadOnly() => cards.AsReadOnly();
    public int Count => cards.Count;
}