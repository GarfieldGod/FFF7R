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
    [SerializeField] private int defaultPageIndex = 0;
    [SerializeField] private CanvasGroup[] pageCanvasGroups;
    [SerializeField] private float pageFadeTime = 0.2f;
    [SerializeField] private bool pageSwitchMix = false;
    [SerializeField] float yOffset = 50;

    private Page[] _pages;
    private int _index = -1;
    Sequence _seq;

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

        SwitchToPage(defaultPageIndex);
    }

    public void SwitchToPage(int index)
    {
        if (index >= _pages.Length || index < 0) return;
        if (index == _index) return;

        var next = _pages[index];
        next.Reset(yOffset);

        _seq?.Kill();
        _seq = DOTween.Sequence();

        if (_index < _pages.Length && _index >= 0)
        {
            var last = _pages[_index];
            _seq.Append(last.canvasGroup.DOFade(0, pageFadeTime));
            _seq.Join(last.rectTransform.DOAnchorPosY(last.basePos.y - yOffset, pageFadeTime));
            last.SetActive(false);
        }

        if (!pageSwitchMix)
            _seq.Append(next.canvasGroup.DOFade(1, pageFadeTime));
        else
            _seq.Join(next.canvasGroup.DOFade(1, pageFadeTime));
        
        _seq.Join(next.rectTransform.DOAnchorPosY(next.basePos.y, pageFadeTime));

        next.SetActive(true);
        _index = index;
    }
}
