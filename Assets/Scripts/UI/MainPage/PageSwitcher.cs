using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

struct Page
{
    public CanvasGroup canvasGroup;
    public RectTransform rectTransform;
    public Vector2 basePos;
    public Page(CanvasGroup canvasGroup, RectTransform rectTransform)
    {
        this.canvasGroup = canvasGroup;
        this.rectTransform = rectTransform;
        basePos = rectTransform.anchoredPosition;
    }

    public void SetActive(bool active)
    {
        canvasGroup.interactable = active;
        canvasGroup.blocksRaycasts = active;
    }

    public void Reset(float yOffset = 0)
    {
        canvasGroup.alpha = 0;
        SetActive(false);
        rectTransform.anchoredPosition = new (basePos.x, basePos.y + yOffset);
    }
}

public class PageSwitcher : MonoBehaviour
{
    [Header("设置")]
    [SerializeField] private int defaultPageIndex = 0;

    [Header("黑幕切换")]
    [SerializeField] private bool useBlackMask = false;
    [SerializeField] private Image blackMask;
    [SerializeField] private float maskFadeTime = 0.15f;

    [Header("浅入浅出切换")]
    [SerializeField] private CanvasGroup[] pageCanvasGroups;
    [SerializeField] private float pageFadeTime = 0.2f;
    [SerializeField] private bool pageSwitchMix = false;
    [SerializeField] float yOffset = 50;

    private Page[] _pages;
    private int _index = -1;

    Sequence _seq;

    public event Action<int, int> OnSwitchComplete;

    void Awake()
    {
        _pages = new Page[pageCanvasGroups.Length];
        for (int i = 0; i < pageCanvasGroups.Length; i++)
        {
            _pages[i] = new Page(pageCanvasGroups[i], pageCanvasGroups[i].GetComponent<RectTransform>());
        }

        foreach(var page in _pages) {
            page.Reset();
        }

        SwitchToPage(defaultPageIndex, false);
    }

    public void SwitchToPage(int index, bool animate = true)
    {
        if (!useBlackMask)
            DoSwitch(index, animate);
        else
            DoBlackMaskSwitch(index, animate);
    }

    public void DoBlackMaskSwitch(int index, bool animate = true)
    {
        if (index >= _pages.Length || index < 0) return;
        if (index == _index) return;
        if (blackMask == null) { DoSwitch(index, animate); return; }

        // 无动画 / 首次进入 → 直接硬切，交给 DoSwitch
        if (!animate || _index < 0)
        {
            DoSwitch(index, false);
            return;
        }

        // 黑幕模式：_index 保持旧值，等全黑那一帧再交给 DoSwitch 更新
        int target = index;

        _seq?.Kill();
        UIUtils.SetAlpha(blackMask, 0f);
        blackMask.raycastTarget = false;

        _seq = DOTween.Sequence().SetLink(gameObject);

        _seq.Append(blackMask.DOFade(1f, maskFadeTime).SetEase(Ease.InQuad));
        _seq.AppendCallback(() => DoSwitch(target, false));      // ← 硬切，不播自带动画
        _seq.Append(blackMask.DOFade(0f, maskFadeTime).SetEase(Ease.OutQuad));

        _seq.OnStart(() => blackMask.raycastTarget = true);
        _seq.OnComplete(() => { blackMask.raycastTarget = false; _seq = null; });
    }

    public void DoSwitch(int index, bool animate = true)
    {
        if (index >= _pages.Length || index < 0) return;
        if (index == _index) return;
        var next = _pages[index];

        if (animate) AnimateSwitch(next);
        else SnapSwitch(next);

        OnSwitchComplete?.Invoke(_index, index);
        _index = index;
    }

    void SnapSwitch(Page next)
    {
        if (_index < _pages.Length && _index >= 0)
        {
            var last = _pages[_index];
            last.canvasGroup.alpha = 0;
            last.SetActive(false);
            last.rectTransform.anchoredPosition = last.basePos;
        }

        next.canvasGroup.alpha = 1;
        next.SetActive(true);
        next.rectTransform.anchoredPosition = next.basePos;
    }

    void AnimateSwitch(Page next)
    {
        _seq?.Kill();
        _seq = DOTween.Sequence();
        next.Reset(-yOffset);

        if (_index < _pages.Length && _index >= 0)
        {
            var last = _pages[_index];
            _seq.Append(last.canvasGroup.DOFade(0, pageFadeTime));
            _seq.Join(last.rectTransform.DOAnchorPosY(last.basePos.y + yOffset, pageFadeTime));
            last.SetActive(false);
        }

        if (!pageSwitchMix)
            _seq.Append(next.canvasGroup.DOFade(1, pageFadeTime));
        else
            _seq.Join(next.canvasGroup.DOFade(1, pageFadeTime));
        
        _seq.Join(next.rectTransform.DOAnchorPosY(next.basePos.y, pageFadeTime));

        next.SetActive(true);
    }
}
