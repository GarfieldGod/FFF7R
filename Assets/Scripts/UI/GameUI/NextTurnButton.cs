using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class NextTurnButton : MonoBehaviour
{
    [SerializeField] CanvasGroup group;

    [SerializeField] float transTime = 0.5f;

    Sequence _seq;

    public void Active(bool act)
    {
        if (act) Show(); else Hide();
    }

    public void Show()
    {
        group.blocksRaycasts = true;
        group.interactable = true;
        _seq?.Kill();

        _seq = DOTween.Sequence();

        _seq.Append(group.DOFade(1f, transTime));
        // _seq.Join(group.transform.DOScaleY(1f, transTime).SetEase(Ease.OutQuad));

        _seq.SetLink(gameObject);
    }

    public void Hide()
    {
        group.blocksRaycasts = false;
        group.interactable = false;
        _seq?.Kill();

        _seq = DOTween.Sequence();
        _seq.Append(group.DOFade(0f, transTime));
        // _seq.Join(group.transform.DOScaleY(0f, transTime).SetEase(Ease.OutQuad));

        _seq.SetLink(gameObject);
    }
}
