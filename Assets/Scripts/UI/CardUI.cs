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
    [SerializeField] float cursorMoveLength = 180f;
    [SerializeField] float cursorMoveTime = 0.5f;

    public CardDefView Def { get; private set; }
    public event Action<string> Selected;

    private CanvasGroup faceCG;

    public void Awake()
    {
        faceCG = face.GetComponent<CanvasGroup>();

        frame.gameObject.SetActive(showFrame);
        frameHighLight.gameObject.SetActive(showFrame);
        Cursor.gameObject.SetActive(showCursor);

        UIUtils.SetAlpha(frameHighLight, 0);
        UIUtils.SetAlpha(Cursor, 0);

        if (showCursor)
        {
            Cursor.rectTransform.DOAnchorPosY(cursorMoveLength, cursorMoveTime)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.Linear)
                    .SetLink(gameObject);
        }
    }

    public void Bind(string cardCode, PlayerSeat seat = PlayerSeat.Player)
    {
        CardDefView def = App.Cards.Get(cardCode);
        Bind(def, seat);
    }

    public void Bind(CardDefView def, PlayerSeat seat = PlayerSeat.None)
    {
        faceCG.DOKill();

        if (def == null) {
            faceCG.DOFade(0, faceFadeTime).SetLink(gameObject);
        }
        else
        {
            faceCG.DOFade(1, faceFadeTime).SetLink(gameObject);
            face.SetCardInHand(def, seat);
        }

        Def = def;
    }

    public void OnPointerClick(PointerEventData e) => Selected?.Invoke(Def != null ? Def.CardCode : null);

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
            Cursor.rectTransform.DOPlay();
        }

        Selected?.Invoke(Def != null ? Def.CardCode : null);
    }

    public void OnPointerExit(PointerEventData e)
    {
        if (showFrame)
        {
            frameHighLight.DOKill();
            frameHighLight.DOFade(0, frameHighLightFadeTime).SetLink(gameObject);;
        }
        if (showCursor)
        {
            UIUtils.SetAlpha(Cursor, 0);
            Cursor.rectTransform.DOPause();
        }

        Selected?.Invoke(null);
    }
}
