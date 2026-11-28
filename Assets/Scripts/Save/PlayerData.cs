using System.Collections.Generic;
using System.Linq;
using FFF7R.Adapter;

public sealed class PlayerData
{
    SaveData _data;
    bool _dirty;

    List<string> _unlockAll;

    public bool DebugUnlockAll { get; set; }

    public IReadOnlyList<string> OwnedCards {
        get
        {
            if (DebugUnlockAll) return UnlockAllCard();
            else return _data.cardData.OwnedCards;
        }
    }

    public IReadOnlyList<string> CardDeck(int index) => _data.cardData.GetDeck(index);

    public void Load()
    {
        _data = SaveStore.Load();
        if (OwnedCards.Count == 0) SeedNewPlayer();
    }

    void SeedNewPlayer()
    {
        var all = App.Cards.AllCodes;
        var firstDeck = new List<string>(DeckRules.DECK_SIZE);
        for (int i = 0; i < all.Count && i < 25; i++)
        {
            var code = all[i];
            _data.cardData.ObtainCard(code);
            if (firstDeck.Count < 15) firstDeck.Add(code);
        }

        SetDeck(0, firstDeck);

        _dirty = true;
        Flush();
    }

    public bool SetDeck(int index, List<string> cards) => _data.cardData.SetDeck(index, cards);

    public bool IsOwned(string code) =>
        DebugUnlockAll || OwnedCards.Contains(code);

    public void ObtainCard(string code)
    {
        if (string.IsNullOrEmpty(code)) return;
        if (!App.VerifyCadeCode(code)) return;

        if (_data.cardData.ObtainCard(code))
            _dirty = true;
    }

    public bool Flush()
    {
        if (!_dirty) return true;
        if (!SaveStore.Save(_data)) return false;
        _dirty = false;
        return true;
    }

    IReadOnlyList<string> UnlockAllCard()
    {
        if (_unlockAll == null)
        {
            _unlockAll = new List<string>();
            foreach (var code in App.Cards.AllCodes)
                for (int i = 0; i < DeckRules.MaxCount(code); i++)
                    _unlockAll.Add(code);
        }
        return _unlockAll.AsReadOnly();
    }
}
