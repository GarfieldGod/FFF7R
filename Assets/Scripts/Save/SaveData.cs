using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

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
    public const int DECK_COUNT = 6;

    public List<string>   owned = new List<string>();
    public List<DeckSlot> decks = new List<DeckSlot>();

    public PlayerCardData()
    {
        for (int i = 0; i < DECK_COUNT; i++)
            decks.Add(new DeckSlot());
    }

    public IReadOnlyList<string> GetDeck(int index)
    {
        if (index < 0 || index >= DECK_COUNT) return null;
        return decks[index].AsReadOnly();
    }

    public void SetDeck(int index, List<string> cards)
    {
        if (index < 0 || index >= DECK_COUNT) return;

        decks[index].Set(cards);
    }

    public IReadOnlyList<string> OwnedCards => owned.AsReadOnly();
    public void ObtainCard(string cardCode) => owned.Add(cardCode);
}

[Serializable]
public class DeckSlot
{
    public const int DECK_SIZE  = 15;

    public string name = string.Empty;
    public List<string> cards = new List<string>();
    public DeckSlot()
    {
        for (int i = 0; i < DECK_SIZE; i++)
            cards.Add(string.Empty);
    }

    public void Set(List<string> newList)
    {
        cards.Clear();
        foreach (var card in newList) cards.Add(card);
    }

    public ReadOnlyCollection<string> AsReadOnly() => cards.AsReadOnly();
}