using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class NavButton : MonoBehaviour, IPointerClickHandler
{
    [Header("选中特效")]
    [SerializeField] private CanvasGroup selectedEffect;
    [SerializeField] private float selectedEffectDuration = 0.5f;
    [SerializeField] private float selectedEffectMaxAlpha = 1f;
    [SerializeField] private float selectedEffectMinAlpha = 0f;

    [Header("文本")]
    [SerializeField] private TMP_Text text;
    [SerializeField] private float textColorDuration = 0.5f;
    [SerializeField] private Color textColorSelected = Color.black;
    [SerializeField] private Color textColorDeselected = Color.white;

    public Action<NavButton> OnSelect { get; set;}

    public void Select(bool animate = true)
    {
        selectedEffect.DOKill();
        if (selectedEffect != null)
        {
            if (animate)
                selectedEffect.DOFade(selectedEffectMaxAlpha, selectedEffectDuration);
            else
                selectedEffect.alpha = selectedEffectMaxAlpha;
        }


        text.DOKill();
        if (text != null)
        {
            if (animate)
                text.DOColor(textColorSelected, textColorDuration);
            else
                text.color = textColorSelected;
        }
    }

    public void Deselect(bool animate = true)
    {
        selectedEffect.DOKill();
        if (selectedEffect != null)
        {
            if (animate)
                selectedEffect.DOFade(selectedEffectMinAlpha, selectedEffectDuration);
            else
                selectedEffect.alpha = selectedEffectMinAlpha;
        }

        text.DOKill();
        if (text != null)
        {
            if (animate)
                text.DOColor(textColorDeselected, textColorDuration);
            else
                text.color = textColorDeselected;
        }
    }

    public void OnPointerClick(PointerEventData pointer) => OnSelect?.Invoke(this);
}