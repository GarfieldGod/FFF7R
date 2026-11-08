using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using FFF7R;
using System;

public class TurnBanner : MonoBehaviour
{
    [SerializeField] float transTime = 0.5f;
    [SerializeField] float stayTime = 2f;

    [SerializeField] Image bgImage;
    [SerializeField] Image textImage;

    public event Action OnComplete;

    Sequence _seq;
    int _requestToken;

    void Awake()
    {
        ResetImage();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Show();
        }
    }

    private void ResetImage()
    {
        bgImage.color = new Color(bgImage.color.r, bgImage.color.g, bgImage.color.b, 0f);
        bgImage.transform.localScale = new Vector3(0.5f, 0.9f, 1);

        textImage.color = new Color(textImage.color.r, textImage.color.g, textImage.color.b, 0f);
        textImage.transform.localScale = new Vector3(0.5f, 0.5f, 1);
    }

    public void Show(PlayerSeat seat)
    {
        if (seat == PlayerSeat.None) return;
        if (this == null) return;

        string bgPic   = seat == PlayerSeat.Player ? "TurnBanner_Player" : "TurnBanner_Rival";
        string textPic = seat == PlayerSeat.Player ? "Text_PlayerTurn"    : "Text_RivalTurn";

        int token = ++_requestToken;      // 本次请求的编号
        _seq?.Kill();                     // 立即收掉上一轮，别等加载

        Sprite bg = null, txt = null;
        int pending = 2;

        void OnOneDone()
        {
            if (--pending > 0) return;
            if (this == null) return;              // 对象已销毁
            if (token != _requestToken) return;    // 已被更新的请求取代
            if (bg == null || txt == null)         // 加载失败，不播
            {
                Debug.LogWarning($"TurnBanner sprite missing: {bgPic} / {textPic}");
                return;
            }

            bgImage.sprite   = bg;
            textImage.sprite = txt;
            Show();
        }

        GameAssets.Sprites.Request(bgPic,   s => { bg  = s; OnOneDone(); });
        GameAssets.Sprites.Request(textPic, s => { txt = s; OnOneDone(); });
    }

    public void UnsafeShow(PlayerSeat seat)
    {
        if (seat == PlayerSeat.None) return;

        string bgPic = seat == PlayerSeat.Player ? "TurnBanner_Player" : "TurnBanner_Rival";
        string textPic = seat == PlayerSeat.Player ? "Text_PlayerTurn" : "Text_RivalTurn";

        GameAssets.Sprites.Request(bgPic, (s) => {
            bgImage.sprite = s;
            GameAssets.Sprites.Request(textPic, (s) => {
                textImage.sprite = s;
                Show();
            });
        });
    }

    private void Show()
    {
        _seq?.Kill();
        ResetImage();

        _seq = DOTween.Sequence();

        // 第一段：淡入 + 展开
        _seq.Append(bgImage.DOFade(1f, transTime));
        _seq.Join(bgImage.transform.DOScaleX(0.9f, transTime).SetEase(Ease.OutQuad));
        _seq.Join(textImage.DOFade(1f, transTime));
        _seq.Join(textImage.transform.DOScale(1f, transTime).SetEase(Ease.OutQuad));

        // 第二段：停留期缓慢漂移
        _seq.Append(bgImage.transform.DOScaleX(0.95f, stayTime).SetEase(Ease.Linear));
        _seq.Join(textImage.transform.DOScale(1.1f, stayTime).SetEase(Ease.Linear));

        // 第三段：淡出
        _seq.Append(bgImage.DOFade(0f, transTime));
        _seq.Join(bgImage.transform.DOScaleY(0f, transTime).SetEase(Ease.InQuad));
        _seq.Join(textImage.DOFade(0f, transTime));
        _seq.Join(textImage.transform.DOScale(1.4f, transTime).SetEase(Ease.InQuad));

        _seq.OnComplete(() => OnComplete?.Invoke());
        _seq.SetLink(gameObject);
    }
}
