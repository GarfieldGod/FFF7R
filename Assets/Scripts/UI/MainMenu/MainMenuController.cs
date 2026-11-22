using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] MainMenu mainMenu;
    [SerializeField] float fadeOutTime = 0.66f;

    [Header("Logo")]
    [SerializeField] TMP_Text logo;
    [SerializeField] float logoYOffset = 10f;
    [SerializeField] float logoFadeInTime = 0.52f;

    [Header("MenuItem")]
    [SerializeField] List<MainMenuItem> menuItems;
    [SerializeField] float menuItemXOffset = 10f;
    [SerializeField] float menuItemFadeInTime = 0.46f;
    [SerializeField] float menuItemStaggerTime = 0.08f;    // 项与项的错开
    [SerializeField] float firstItemDelay = 0.22f;

    [Header("Version")]
    [SerializeField] TMP_Text versionText;
    [SerializeField] float versionFadeInTime = 0.5f;

    [Header("HighLight")]
    [SerializeField] Image highlight;
    [SerializeField] float highlightFadeInTime = 0.3f;
    [SerializeField] float highlightAlpha = 0.8f;
    [SerializeField] float highlightAlphaMin = 0.6f;
    [SerializeField] float highlightBreathTime = 1.4f;
    [SerializeField] float highlightSlideTime = 0.1f;

    private RectTransform logoRect;
    private RectTransform highlightRect;
    private List<RectTransform> menuItemsRect = new List<RectTransform>();
    private List<CanvasGroup> menuItemsGroup = new List<CanvasGroup>();
    private Sequence _seq;
    private int _index = 0;
    private bool _ready = false;
    private float logoYStart;
    private float menuItemXStart;

    public int Index => _index;

    void Awake()
    {
        logoRect = logo.GetComponent<RectTransform>();
        highlightRect = highlight.GetComponent<RectTransform>();
        foreach(var item in menuItems)
        {
            menuItemsGroup.Add(item.GetComponent<CanvasGroup>());
            menuItemsRect.Add(item.GetComponent<RectTransform>());
        }

        for (int i = 0; i < menuItems.Count; i++) menuItems[i].Init(this, i);

        logoYStart = logoRect.anchoredPosition.y;
        menuItemXStart = menuItemsRect[0].anchoredPosition.x;
    }

    void Start()
    {
        AnimeIn();
    }

    void AnimeIn()
    {
        for (int k = 0; k < menuItems.Count; k++) menuItems[k].SetSelected(false, false);
        // 清空状态
        _seq?.Kill();
        _seq = DOTween.Sequence();

        logoRect.anchoredPosition = new Vector2(logoRect.anchoredPosition.x, logoYStart - logoYOffset);
        logo.alpha = 0f;

        for (int i = 0; i < menuItemsGroup.Count; i++)
        {
            menuItemsRect[i].anchoredPosition = new Vector2(menuItemsRect[i].anchoredPosition.x - menuItemXOffset, menuItemsRect[i].anchoredPosition.y);
            menuItemsGroup[i].alpha = 0f;
        }

        versionText.DOKill();
        versionText.alpha = 0f;

        highlight.color = new Color(highlight.color.r, highlight.color.g, highlight.color.b, 0);

        // 开始动画
        _seq.Append(logoRect.DOAnchorPosY(logoYStart, logoFadeInTime))
            .Join(logo.DOFade(1f, logoFadeInTime));

        for (int i = 0; i < menuItemsGroup.Count; i++)
        {
            float at = firstItemDelay + i * menuItemStaggerTime;
            _seq.Insert(at, menuItemsGroup[i].DOFade(1f, menuItemFadeInTime));
            _seq.Insert(at, menuItemsRect[i].DOAnchorPosX(menuItemXStart, menuItemFadeInTime));
        }

        float hlAt = firstItemDelay + (menuItemStaggerTime * (menuItems.Count - 1)) + menuItemFadeInTime;
        _seq.Insert(hlAt, highlight.DOFade(highlightAlpha, highlightFadeInTime));
        menuItems[0].InsertSelectedInto(_seq, hlAt, true, highlightFadeInTime);

        _seq.Insert(hlAt, versionText.DOFade(1f, versionFadeInTime));

        _seq.OnComplete(() => {_ready = true; StartHighLightBreath();});
        _seq.SetLink(gameObject);
    }

    void StartHighLightBreath()
    {
        highlight.DOKill();
        highlight.color = new Color(highlight.color.r, highlight.color.g, highlight.color.b, highlightAlpha);
        highlight.DOFade(highlightAlphaMin, highlightBreathTime)   // 1.0 → 0.82
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine)
            .SetLink(gameObject);
    }

    void AnimeOut(int i)
    {
        _seq?.Kill();
        _seq = DOTween.Sequence();

        highlight.DOKill();

        _seq.Append(logoRect.DOAnchorPosY(logoYStart - logoYOffset, fadeOutTime))
            .Join(logo.DOFade(0f, fadeOutTime));

        for (int j = 0; j < menuItems.Count; j++)
        {
            if (j != i) {
                _seq.Join(menuItemsRect[j].DOAnchorPosX(menuItemXStart - menuItemXOffset, fadeOutTime));
                _seq.Join(menuItemsGroup[j].DOFade(0f, fadeOutTime));
                continue;
            }
        }

        _seq.Join(versionText.DOFade(0f, fadeOutTime));
        _seq.Insert(0, highlight.DOFade(1f, fadeOutTime / 2));
        _seq.Insert(fadeOutTime / 2, highlight.DOFade(highlightAlpha, fadeOutTime / 2));
        _seq.Insert(fadeOutTime, highlight.DOFade(0f, menuItemFadeInTime));

        _seq.Insert(fadeOutTime, menuItemsRect[i].DOAnchorPosX(menuItemXStart + menuItemXOffset, menuItemFadeInTime));
        _seq.Insert(fadeOutTime, menuItemsGroup[i].DOFade(0f, menuItemFadeInTime));
    }

    public void Hover(int i)
    {
        Debug.Log("Hover: " + i + " ready: " + _ready);
        if (!_ready) return;

        i = Mathf.Clamp(i, 0, menuItems.Count - 1);
        _index = i;

        // 设置选择效果
        for (int k = 0; k < menuItems.Count; k++) menuItems[k].SetSelected(k == i, true);

        // 移动高光背景
        float y = menuItems[i].Height;
        highlightRect.DOKill();
        highlight.DOKill();
        highlightRect.DOAnchorPosY(y, highlightSlideTime)
                    .SetEase(Ease.OutCubic)
                    .OnComplete(() => StartHighLightBreath())
                    .SetLink(gameObject);
    }

    public void Click(int i)
    {
        _ready = false;
        AnimeOut(i);
    }

    void Update()
    {
        if (!_ready) return;
        if (Input.GetKeyDown(KeyCode.DownArrow)) Hover(_index + 1);
        if (Input.GetKeyDown(KeyCode.UpArrow))   Hover(_index - 1);
        if (Input.GetKeyDown(KeyCode.Return))    Click(_index);
    }
}