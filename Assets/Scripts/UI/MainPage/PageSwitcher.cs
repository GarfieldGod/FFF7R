using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PageSwitcher : MonoBehaviour
{
    [SerializeField] private int defaultPageIndex = 0;
    [SerializeField] private CanvasGroup[] pages;
    [SerializeField] private float pageFadeTime = 0.2f;
    [SerializeField] private bool pageSwitchMix = false;

    private CanvasGroup _current = null;
    Sequence _seq;

    void Awake()
    {
        foreach(var page in pages)
        {
            page.alpha = 0;
            page.interactable = false;
            page.blocksRaycasts = false;
        }

        SwitchToPage(defaultPageIndex);
    }

    public void SwitchToPage(int index)
    {
        if (index >= pages.Length || index < 0) return;

        var page = pages[index];
        if (page == null || page == _current) return;

        if (!pageSwitchMix)
        {
            _seq?.Kill();
            _seq = DOTween.Sequence();
            if (_current != null) _seq.Append(_current.DOFade(0, pageFadeTime));
            _seq.Append(page.DOFade(1, pageFadeTime));
        } else
        {
            if (_current != null)
            {
                _current.DOKill();
                _current.DOFade(0, pageFadeTime); 
            }
            page.DOKill();
            page.DOFade(1, pageFadeTime);
        }

        if (_current != null)
        {
            _current.interactable = false;
            _current.blocksRaycasts = false;
        }
        page.interactable = true;
        page.blocksRaycasts = true;

        _current = page;
    }
}
