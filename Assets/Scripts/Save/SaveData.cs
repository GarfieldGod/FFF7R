using System;
using System.Collections.Generic;

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
    private readonly List<string> _owned = new List<string>();
    private readonly List<List<string>> _decks = new List<List<string>>();
    public static readonly int DECK_COUNT = 6;
    public PlayerCardData()
    {
        for (int i = 0; i < DECK_COUNT; i++)
            _decks.Add(new List<string>());
    }

    public IReadOnlyList<string> GetDeck(int index)
    {
        if (index < 0 || index >= DECK_COUNT) return null;
        return _decks[index].AsReadOnly();
    }

    public void SetDeck(int index, List<string> cards)
    {
        if (index < 0 || index >= DECK_COUNT) return;

        _decks[index].Clear();
        foreach (var card in cards) _decks[index].Add(card);
    }

    public IReadOnlyList<string> OwnedCards => _owned.AsReadOnly();
    public void ObtainCard(string cardCode) => _owned.Add(cardCode);
}