using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

[RequireComponent(typeof(Button))]
public class MainMenuItem : MonoBehaviour, IPointerEnterHandler
{
    public float Height => _rect.anchoredPosition.y;

    [SerializeField] private Image cursor;
    [SerializeField] private Image cursorSelected;
    [SerializeField] private TMP_Text text;
    [SerializeField] float slideTime = 0.1f;
    [SerializeField] Material glowMat;
    [SerializeField] Material normalMat;

    private RectTransform _textRect;
    private RectTransform _rect;
    private MainMenuController _mainMenuController;
    private int _index = -1;
    private Button _btn;

    void Awake()
    {
        _rect = GetComponent<RectTransform>();
        _btn = GetComponent<Button>();
        _btn.onClick.AddListener(OnClick);

        _textRect = text.GetComponent<RectTransform>();
    }

    public void Init(MainMenuController mainMenu, int index)
    {
        if (_mainMenuController != null && _mainMenuController != mainMenu) return;
        _mainMenuController = mainMenu;
        _index = index;

        Clear();
    }

    private void Clear()
    {
        text.DOKill();
        _textRect.DOKill();
        cursor.DOKill();
        cursorSelected.DOKill();
    }

    public void SetSelected(bool selected, bool animate)
    {
        Clear();
        float x = selected ? 10 : 0;
        float alphaN = selected ? 0 : 107/255f;
        float alphaS = selected ? 1 : 0;
        float alphaT = selected ? 1 : 107/255f;
        if (animate)
        {
            _textRect.DOAnchorPosX(x, slideTime).SetEase(Ease.OutCubic);
            cursor.DOFade(alphaN, slideTime);
            cursorSelected.DOFade(alphaS, slideTime);
            text.DOFade(alphaT, slideTime);
        }
        else
        {
            _textRect.anchoredPosition = new Vector2(x, _textRect.anchoredPosition.y);
            cursor.color = new Color(cursor.color.r, cursor.color.g, cursor.color.b, alphaN);
            cursorSelected.color = new Color(cursorSelected.color.r, cursorSelected.color.g, cursorSelected.color.b, alphaS);
            text.color = new Color(text.color.r, text.color.g, text.color.b, alphaT);
        }

        text.fontSharedMaterial = selected ? glowMat : normalMat;
    }

    public void InsertSelectedInto(Sequence seq, float at, bool selected, float dur)
    {
        float x      = selected ? 10f : 0f;
        float alphaN = selected ? 0f : 107f / 255f;
        float alphaS = selected ? 1f : 0f;
        float alphaT = selected ? 1f : 107f / 255f;

        seq.Insert(at, _textRect.DOAnchorPosX(x, dur).SetEase(Ease.OutCubic));
        seq.Insert(at, cursor.DOFade(alphaN, dur));
        seq.Insert(at, cursorSelected.DOFade(alphaS, dur));
        seq.Insert(at, text.DOFade(alphaT, dur));

        text.fontSharedMaterial = selected ? glowMat : normalMat;
    }

    public void OnClick() => _mainMenuController.Click(_index);

    public void OnPointerEnter(PointerEventData eventData) => _mainMenuController.Hover(_index);
}