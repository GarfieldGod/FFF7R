using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using FFF7R;
using TMPro;
using UnityEngine;

public class CardDetailPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text typeText;
    [SerializeField] private TMP_Text codeText;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text detailText;
    [SerializeField] private CardUI cardUI;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeTime = 0.2f;

    void Start() => canvasGroup.alpha = 0;

    public void ShowCardDetail(string cardCode)
    {
        if (cardCode == null) {
            HidePanel();
            return;
        }

        CardDefView def = App.Cards.Get(cardCode);
        if (def == null) {
            HidePanel();
            return;
        }

        ShowPanel(def);
    }

    public void HidePanel()
    {
        canvasGroup.DOKill();
        canvasGroup.DOFade(0, fadeTime).SetLink(gameObject);
    }

    public void ShowPanel(CardDefView def)
    {
        canvasGroup.DOKill();
        canvasGroup.DOFade(1, fadeTime).SetLink(gameObject);

        cardUI.Bind(def);
        codeText.text = "卡片No." + def.CardCode.Substring(4);
        switch (def.Type) {
            case CardType.Advance:
                typeText.text = "传说";
                break;
            default:
                typeText.text = "普通";
                break;
        }
        nameText.text = def.Name;
        detailText.text = def.Description;
    }
}
