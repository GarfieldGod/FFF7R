using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardPage : MonoBehaviour
{
    [SerializeField] CardDeckPanel deckPanel;
    [SerializeField] CardDetailPanel detailPanel;
    // Start is called before the first frame update
    void Awake()
    {
        deckPanel.OnCardSelected += detailPanel.ShowCardDetail;
    }

    void OnDestroy()
    {
        deckPanel.OnCardSelected -= detailPanel.ShowCardDetail;
    }
}