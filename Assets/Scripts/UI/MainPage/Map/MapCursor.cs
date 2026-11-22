using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class MapCursor : MonoBehaviour
{
    [Header("全局设置")]
    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] float _fadeInTime = 0.5f;
    [SerializeField] float _baseAlpha = 1f;
    [SerializeField] float effectBreathTime = 2f;

    [Header("光标设置")]
    [SerializeField] Image cursorImage;
    [SerializeField] Image cursorImageRotInside;
    [SerializeField] Image cursorImageRotOutside;
    [SerializeField] float rotInsideTime  = 8f;    // 内圈转一圈的秒数
    [SerializeField] float rotOutsideTime = 5f;    // 外圈转一圈的秒数
    [SerializeField] bool  insideClockwise = true; // 内圈顺时针，外圈自动反向
    [SerializeField] bool  outsideClockwise = true; // 内圈顺时针，外圈自动反向

    [Header("内圈特效设置")]
    [SerializeField] Image cursorEffectInside;
    [SerializeField] float effectInsideMaxAlpha = 0.3f;
    [SerializeField] float effectInsideBaseScale = 1f;
    [SerializeField] float effectInsideMaxScale = 1.2f;
    [SerializeField] float effectInsideFadeInTime = 0.5f;
    [SerializeField] float effectInsideFadeOutTime = 0.5f;

    [Header("外圈特效设置")]
    [SerializeField] Image cursorEffectOutside;
    [SerializeField] float effectOutsideMaxAlpha = 0.3f;
    [SerializeField] float effectOutsideBaseScale = 0.7f;
    [SerializeField] float effectOutsideMaxScale = 1.4f;
    [SerializeField] float effectOutsideFadeInTime = 0.5f;
    [SerializeField] float effectOutsideScaleTime = 0.5f;
    [SerializeField] float effectOutsideFadeOutTime = 0.5f;

    RectTransform _rect;

    void Awake()
    {
        _rect = GetComponent<RectTransform>();
        canvasGroup.alpha = 0f;
    }

    void Start()
    {
        StartSpin(cursorImageRotInside.rectTransform, rotInsideTime, insideClockwise);
        StartSpin(cursorImageRotOutside.rectTransform, rotOutsideTime, outsideClockwise);
        StartEffect();

        // EnableCursor(Vector2.zero); // 测试用
    }

    void StartSpin(RectTransform rt, float duration, bool clockwise)
    {
        if (rt == null) return;

        rt.DOKill();
        rt.localRotation = Quaternion.identity;

        rt.DOLocalRotate(
                new Vector3(0f, 0f, clockwise ? 360f : -360f),
                duration,
                RotateMode.FastBeyond360)
          .SetEase(Ease.Linear)
          .SetLoops(-1, LoopType.Restart) // 无限循环
          .SetLink(gameObject);
    }

    Sequence _seq;
    void StartEffect()
    {
        _seq?.Kill();
        UIUtils.SetAlpha(cursorEffectInside, 0);
        UIUtils.SetAlpha(cursorEffectOutside, 0);
        cursorEffectInside.rectTransform.localScale = Vector3.one * effectInsideBaseScale;
        cursorEffectOutside.rectTransform.localScale = Vector3.one * effectOutsideBaseScale;

        _seq = DOTween.Sequence();
        _seq.Append(cursorEffectInside.DOFade(effectInsideMaxAlpha, effectInsideFadeInTime));
        _seq.Join(cursorEffectInside.rectTransform.DOScale(effectInsideMaxScale, effectInsideFadeInTime));
    
        _seq.Append(cursorEffectInside.DOFade(0f, effectInsideFadeOutTime));

        float outSideStartAt = 0;
        _seq.Insert(outSideStartAt,
            cursorEffectOutside.DOFade(effectOutsideMaxAlpha, effectOutsideFadeInTime));
        _seq.Insert(outSideStartAt + effectOutsideFadeInTime,
            cursorEffectOutside.rectTransform.DOScale(effectOutsideMaxScale, effectOutsideScaleTime));
        _seq.Insert(outSideStartAt + effectOutsideFadeInTime + effectOutsideScaleTime/2,
            cursorEffectOutside.DOFade(0f, effectOutsideFadeOutTime));

        _seq.AppendInterval(effectBreathTime);
        _seq.SetLink(gameObject)
            .SetLoops(-1, LoopType.Restart);
    }

    public void EnableCursor(Vector2 pos)
    {
        _rect.anchoredPosition = pos;

        canvasGroup.DOKill();
        canvasGroup.DOFade(_baseAlpha, _fadeInTime).SetLink(gameObject);

        cursorImageRotInside.rectTransform.DOPlay();
        cursorImageRotOutside.rectTransform.DOPlay();

        cursorEffectInside.DOPlay();
        cursorEffectOutside.DOPlay();
        cursorEffectInside.rectTransform.DOPlay();
        cursorEffectOutside.rectTransform.DOPlay();
    }

    public void DisableCursor()
    {
        canvasGroup.DOKill();
        canvasGroup.DOFade(0, _fadeInTime).SetLink(gameObject);

        cursorImageRotInside.rectTransform.DOPause();
        cursorImageRotOutside.rectTransform.DOPause();

        cursorEffectInside.DOPause();
        cursorEffectOutside.DOPause();
        cursorEffectInside.rectTransform.DOPause();
        cursorEffectOutside.rectTransform.DOPause();
    }
}