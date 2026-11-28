using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PageSwitcher))]
public class CardPage : MonoBehaviour
{
    [Header("查看页")]
    [SerializeField] CardDeckPanel deckPanel;
    [SerializeField] CardDetailPanel detailPanel;
    [SerializeField] NavButton editButton;

    [Header("编辑页")]
    [SerializeField] CardDeckEditor editPanel;
    [SerializeField] NavButton SaveButton;

    [Header("HUD")]
    [SerializeField] CanvasGroup hud;

    private PageSwitcher pageSwitcher;
    // Start is called before the first frame update
    void Awake()
    {
        pageSwitcher = GetComponent<PageSwitcher>();
        pageSwitcher.SwitchToPage(0);

        deckPanel.OnCardSelected += detailPanel.ShowCardDetail;
        editButton.OnClick += Edit;
        SaveButton.OnClick += Save;
    }

    void OnDestroy()
    {
        deckPanel.OnCardSelected -= detailPanel.ShowCardDetail;
        editButton.OnClick -= Edit;
        SaveButton.OnClick -= Save;
    }

    void Edit(NavButton button)
    {
        editPanel.EditDeck(deckPanel.CurrentDeckIndex);
        pageSwitcher.SwitchToPage(1);
        SetHud(false);
    }

    void Save(NavButton button)
    {
        bool success = editPanel.SaveDeck();
        if (success)
        {
            pageSwitcher.SwitchToPage(0);
            deckPanel.Flush();
            SetHud(true);
        }
    }

    void SetHud(bool visible)
    {
        hud.alpha = visible ? 1 : 0;
        hud.interactable = visible;
        hud.blocksRaycasts = visible;
    }
}