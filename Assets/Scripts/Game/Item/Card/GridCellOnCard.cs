using UnityEngine;
using UnityEngine.UI;

public class GridCellOnCard : MonoBehaviour
{
    public enum State
    {
        Empty,
        Center,
        PosEffect,
        CardEffect,
        PosEffectAndCardEffect
    }

    [SerializeField] private Image _baseImage;
    [SerializeField] private Image _frameImage;

    private static readonly Color PosEffectColor = new Color(238/255f, 179/255f, 101/255f);
    private static readonly Color EmptyColor = new Color(89/255f, 72/255f, 51/255f);
    private static readonly Color CardEffectColor = new Color(196/255f, 93/255f, 61/255f);

    public void Init()
    {
        _baseImage.color = EmptyColor;
        _frameImage.color = CardEffectColor;
        UIUtils.SetAlpha(_frameImage, 0);
    }

    public void SetState(State state)
    {
        bool hasFrame = state == State.CardEffect || state == State.PosEffectAndCardEffect;
        UIUtils.SetAlpha(_frameImage, hasFrame ? 1 : 0);

        switch (state)
        {
            case State.Center:
                _baseImage.color = Color.white;
                break;
            case State.Empty:
                _baseImage.color = EmptyColor;
                break;
            case State.PosEffect:
            case State.PosEffectAndCardEffect:
                _baseImage.color = PosEffectColor;
                break;
            default:
                break;
        }
    }
}
