using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] Image highlight;
    [SerializeField] List<MainMenuItem> items;
    [SerializeField] float slideTime = 0.16f;
    [SerializeField] float highlightAlphaMax = 1.4f;
    [SerializeField] float highlightAlphaMin = 0.82f;

    public CanvasGroup Group;

    int _index;
    RectTransform highlightRect;

    void Start()
    {
        Group = GetComponent<CanvasGroup>();
        highlightRect = highlight.GetComponent<RectTransform>();
        for (int i = 0; i < items.Count; i++) items[i].Init(this, i);
        Apply(0, false);
    }

    public void Hover(int i) => Apply(i, true);

    void Apply(int i, bool animate)
    {
        i = Mathf.Clamp(i, 0, items.Count - 1);
        _index = i;

        // 设置选择效果
        for (int k = 0; k < items.Count; k++) items[k].SetSelected(k == i, animate);

        // 移动高光背景
        float y = items[i].Height;
        highlightRect.DOKill();
        highlight.DOKill();
        if (animate) {
            highlightRect.DOAnchorPosY(y, slideTime).SetEase(Ease.OutCubic).SetLink(gameObject);
            highlight.DOFade(highlightAlphaMin, highlightAlphaMax)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine)
                .SetLink(gameObject);
        }
        else
            highlightRect.anchoredPosition = new Vector2(highlightRect.anchoredPosition.x, y);
    }

    void Active(int i)
    {
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow)) Apply(_index + 1, true);
        if (Input.GetKeyDown(KeyCode.UpArrow))   Apply(_index - 1, true);
        if (Input.GetKeyDown(KeyCode.Return))    Active(_index);
    }
}