using System;
using UnityEngine;
using UnityEngine.EventSystems;
using FFF7R;
using UnityEngine.UI;
using DG.Tweening;

public class CardUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("CardFace")]
    [SerializeField] CardFace face;
    [SerializeField] float faceFadeTime = 0.2f;

    [Header("CardFrame")]
    [SerializeField] bool showFrame = true;
    [SerializeField] Image frame;
    [SerializeField] Image frameHighLight;
    [SerializeField] float frameHighLightFadeTime = 0.2f;

    [Header("CardBackground")]
    [SerializeField] Image Background;

    [Header("CardCursor")]
    [SerializeField] bool showCursor = true;
    [SerializeField] Image Cursor;
    [SerializeField] float cursorMoveLength = 20f;
    [SerializeField] float cursorMoveTime = 0.5f;

    public CardDefView Def { get; private set; }
    public string CardCode => Def != null ? Def.CardCode : null;
    public event Action<string> Selected;
    public event Action<string> Clicked;

    private CanvasGroup faceCG;
    private Vector2 _cursorBase;
    private Tweener _cursorTween;

    public void Awake()
    {
        faceCG = face.GetComponent<CanvasGroup>();
        _cursorBase = Cursor.rectTransform.anchoredPosition;

        frame.gameObject.SetActive(showFrame);
        frameHighLight.gameObject.SetActive(showFrame);
        Cursor.gameObject.SetActive(showCursor);

        UIUtils.SetAlpha(frameHighLight, 0);
        UIUtils.SetAlpha(Cursor, 0);
    }

    void OnDisable()
    {
        if (showCursor && Cursor != null)
        {
            Cursor.rectTransform.DOKill();
            Cursor.rectTransform.anchoredPosition = _cursorBase;
            UIUtils.SetAlpha(Cursor, 0);
        }
    }

    public void Bind(string cardCode, PlayerSeat seat = PlayerSeat.Player)
    {
        CardDefView def = App.Cards.Get(cardCode);
        Bind(def, seat);
    }

    public void Bind(CardDefView def, PlayerSeat seat = PlayerSeat.None, bool animate = false)
    {
        faceCG.DOKill();

        float target = def == null ? 0 : 1;
        if (!animate || Mathf.Approximately(faceCG.alpha, target))
            faceCG.alpha = target;                                    // 直接赋值
        else
            faceCG.DOFade(target, faceFadeTime).SetLink(gameObject);

        if (def != null) {
            face.SetCardInHand(def, seat);
        }

        Def = def;
    }

    public void OnPointerClick(PointerEventData e) => Clicked?.Invoke(Def != null ? Def.CardCode : null);

    public void OnPointerEnter(PointerEventData e)
    {
        if (showFrame)
        {
            frameHighLight.DOKill();
            frameHighLight.DOFade(1, frameHighLightFadeTime).SetLink(gameObject);;
        }

        if (showCursor)
        {
            UIUtils.SetAlpha(Cursor, 1);

            if (_cursorTween == null)
            {
                Cursor.rectTransform.anchoredPosition = _cursorBase;
                _cursorTween = Cursor.rectTransform
                    .DOAnchorPosY(_cursorBase.y + cursorMoveLength, cursorMoveTime)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.Linear)
                    .SetLink(gameObject, LinkBehaviour.PauseOnDisableRestartOnEnable)
                    .Pause();
            }
            _cursorTween.Restart();
        }

        Selected?.Invoke(Def != null ? Def.CardCode : null);
    }

    public void OnPointerExit(PointerEventData e)
    {
        if (showFrame)
        {
            frameHighLight.DOKill();
            frameHighLight.DOFade(0, frameHighLightFadeTime).SetLink(gameObject);
        }
        if (showCursor)
        {
            UIUtils.SetAlpha(Cursor, 0);
            _cursorTween?.Pause();
            Cursor.rectTransform.anchoredPosition = _cursorBase;
        }

        Selected?.Invoke(null);
    }
}
