using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class NavButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("点击特效")]
    [SerializeField] private CanvasGroup selectedEffect;
    [SerializeField] private float selectedEffectDuration = 0.5f;
    [SerializeField] private float selectedEffectMaxAlpha = 1f;
    [SerializeField] private float selectedEffectMinAlpha = 0f;

    [Header("悬浮特效")]
    [SerializeField] private CanvasGroup hoverEffect;
    [SerializeField] private float hoverEffectDuration = 0.5f;
    [SerializeField] private float hoverEffectMaxAlpha = 1f;
    [SerializeField] private float hoverEffectMinAlpha = 0f;

    [Header("文本")]
    [SerializeField] private TMP_Text text;
    [SerializeField] private float textColorDuration = 0.5f;
    [SerializeField] private Color textColorSelected = Color.black;
    [SerializeField] private Color textColorDeselected = Color.white;

    public event Action<NavButton> OnClick;

    public void Awake()
    {
        if (selectedEffect != null) selectedEffect.alpha = selectedEffectMinAlpha;
        if (hoverEffect != null) hoverEffect.alpha = hoverEffectMinAlpha;
        if (text != null) text.color = textColorDeselected;
    }

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

    public void OnPointerClick(PointerEventData pointer) => OnClick?.Invoke(this);

    public void OnPointerEnter(PointerEventData pointer)
    {
        hoverEffect.DOKill();
        if (hoverEffect != null)
            hoverEffect.DOFade(hoverEffectMaxAlpha, hoverEffectDuration);
    }

    public void OnPointerExit(PointerEventData pointer)
    {
        hoverEffect.DOKill();
        if (hoverEffect != null)
            hoverEffect.DOFade(hoverEffectMinAlpha, hoverEffectDuration);
    }
}