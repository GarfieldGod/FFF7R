using System;
using DG.Tweening;
using FFF7R;
using UnityEngine;
using UnityEngine.UI;

public class ReDispensePanel : MonoBehaviour
{
    [SerializeField] private Button rDButton;
    [SerializeField] private CanvasGroup group;
    [SerializeField] private float transTime = 0.5f;

    public event Action ReDispenseRequested;
    // Start is called before the first frame update
    private Sequence _seq;

    void Awake()
    {
        rDButton.onClick.AddListener(ReDispense);
    }

    void OnDestroy()
    {
        rDButton.onClick.RemoveListener(ReDispense);
    }

    void ReDispense()
    {
        ReDispenseRequested?.Invoke();
    }

    public void OnStageChanged(MatchStatus s)
    {
        if (s == MatchStatus.ReDispense)
        {
            Show();
        } else if (s == MatchStatus.Gaming)
        {
            Hide();
        }
    }

    private void Show()
    {
        _seq?.Kill();

        _seq = DOTween.Sequence();

        // 第一段：淡入 + 展开
        _seq.Append(group.DOFade(1f, transTime));
        _seq.SetLink(gameObject);

        group.interactable = true;
        group.blocksRaycasts = true;
    }

    private void Hide()
    {
        _seq?.Kill();

        _seq = DOTween.Sequence();

        // 第一段：淡入 + 展开
        _seq.Append(group.DOFade(0f, transTime));
        _seq.SetLink(gameObject);

        group.interactable = false;
        group.blocksRaycasts = false;
    }
}
