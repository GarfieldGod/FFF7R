using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PageSwitcher : MonoBehaviour
{
    [SerializeField] private NavUI nav;
    [SerializeField] private int defaultPageIndex = 0;
    [SerializeField] private CanvasGroup[] pages;

    private CanvasGroup _current = null;

    void Start()
    {
        nav.OnNavButtonSelected += OnPageChange;
        foreach(var page in pages)
        {
            page.alpha = 0;
            page.interactable = false;
            page.blocksRaycasts = false;
        }

        OnPageChange(defaultPageIndex);
    }

    void OnPageChange(int index)
    {
        if (index >= pages.Length || index < 0) return;

        var page = pages[index];
        if (page == null || page == _current) return;

        if (_current != null)
        {
            // _current.alpha = 0;
            _current.DOFade(0, 0.5f);
            _current.interactable = false;
            _current.blocksRaycasts = false;
        }

        // page.alpha = 1;
        page.DOFade(1, 0.5f);
        page.interactable = true;
        page.blocksRaycasts = true;
        _current = page;
    }
}
