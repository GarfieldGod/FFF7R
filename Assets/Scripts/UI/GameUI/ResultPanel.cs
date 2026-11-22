using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FFF7R;
using DG.Tweening;

public class ResultPanel : MonoBehaviour
{
    [SerializeField] private CanvasGroup panel;
    [SerializeField] private Image resultText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private PlayerSeat seat = PlayerSeat.Player;
    [SerializeField] float transTime = 0.5f;

    Sequence _seq;

    private void Awake()
    {
        Hide();
    }

    public void OnMatchEnd(IGameQuery q) {
        bool myWin = seat == q.State.Winner;
        GameAssets.Sprites.Request(myWin ? "Text_Win" : "Text_Lose", (s) =>
        {
            resultText.sprite = s;
            scoreText.text = $"Player: {q.Board.PlayerScore}                                 {q.Board.RivalScore} :Rival";
            Show();
        });
    }

    public void Show() {
        panel.blocksRaycasts = true;
        panel.interactable = true;
        _seq?.Kill();

        _seq = DOTween.Sequence();
        _seq.Append(panel.DOFade(1f, transTime));

        _seq.SetLink(gameObject);
    }

    public void Hide()
    {
        panel.alpha = 0;
    }
}
