using System;
using System.Collections;
using System.Collections.Generic;
using FFF7R;
using UnityEngine;

public class CardSelector : MonoBehaviour
{
    public Card cardPrefab;
    public float CardSelectorLength = -25;
    public float CardSelectorHeight = 1;
    public List<Card> cards = new List<Card>();

    // Start is called before the first frame update
    void Start()
    {

    }

    public void UpdateHand(HandSnapshot hand)
    {
        cards.Clear();
        foreach (CardView cardView in hand.Cards)
        {
            AddCard(cardView.CardCode);
        }
    }

    public void AddCard(string cardCode)
    {
        Card card = Instantiate(cardPrefab, transform);
        card.SetCard(cardCode);

        cards.Add(card);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateCardsPos();
    }

    void UpdateCardsPos()
    {
        if (cards.Count == 0) return;

        float spacingX = CardSelectorLength / cards.Count;
        float spacingZ = CardSelectorHeight / cards.Count;
        for (int i = 0; i < cards.Count; i++)
        {
            float x = spacingX * i;
            float y = 0;
            float z = spacingZ * i;
            cards[i].transform.localPosition = new Vector3(x, y, z);
        }
    }
}
