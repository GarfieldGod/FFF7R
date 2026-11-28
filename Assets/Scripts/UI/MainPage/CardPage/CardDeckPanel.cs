using System;
using System.Linq;
using FFF7R;
using UnityEngine;

public class CardDeckPanel : MonoBehaviour
{
    [SerializeField] private NavUI cardDeckNav;
    [SerializeField] private CardUI[] cardUI;
    [SerializeField] private DeckInfoPanel deckInfoPanel;

    public event Action<string> OnCardSelected;

    public int CurrentDeckIndex = -1;
    // Start is called before the first frame update
    void Awake()
    {
        cardDeckNav.OnNavButtonSelected += ShowDeck;

        for (int i = 0; i < cardUI.Length; i++)
            cardUI[i].Selected += SelectCard;
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
        if (CurrentDeckIndex == index) return;
        CurrentDeckIndex = index;

        Flush();
    }

    public void Flush()
    {
        var cardList = App.Save.CardDeck(CurrentDeckIndex);
        if (cardList == null) return;

        var sorted = cardList.OrderBy(App.Cards.RankOf).ToList();
    
        int n = Mathf.Min(sorted.Count, cardUI.Length);
        for (int i = 0; i < n; i++)
            cardUI[i].Bind(sorted[i]);

        for (int i = n; i < cardUI.Length; i++)
            cardUI[i].Bind((CardDefView)null);

        deckInfoPanel.SetDeckInfo(CurrentDeckIndex, cardList.Count);
    }
}
