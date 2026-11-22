using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DetailText : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    [SerializeField] private CanvasGroup group;

    [Header("动画")]
    [SerializeField] private float fadeInTime  = 0.12f;
    [SerializeField] private float holdTime    = 0.5f;   // 移开后的残留
    [SerializeField] private float fadeOutTime = 0.2f;

    private Tween _fade;

    public void SetText(string content)
    {
        if (string.IsNullOrEmpty(content))
        {
            if (string.IsNullOrEmpty(text.text)) return;   // 本来就空，无事可做

            _fade?.Kill();
            _fade = text.DOFade(0f, fadeOutTime)
                        .SetDelay(holdTime)               // Kill 会连延迟段一起取消
                        .SetUpdate(true)                  // 不受 timeScale 影响
                        .OnComplete(() =>
                        {
                            text.text  = string.Empty;
                            text.alpha = 0f;
                        });
            return;
        }

        _fade?.Kill();
        if (text.text != content) text.text = content;
        _fade = text.DOFade(1f, fadeInTime).SetUpdate(true);   // 从当前 alpha 淡入，不重置
    }

    public void Hide()
    {
        group.alpha = 0f;
    }

    public void Show()
    {
        group.alpha = 1f;
    }

    void OnDisable()
    {
        _fade?.Kill();
        text.text  = string.Empty;
        text.alpha = 0f;
    }
}
