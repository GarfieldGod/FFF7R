using UnityEngine;
using UnityEngine.UI;

public class GridCellOnCard : MonoBehaviour
{
    private Material _tileMat;

    private static readonly Color PosEffectColor = new Color(238/255f, 179/255f, 101/255f);
    private static readonly Color EmptyColor = new Color(89/255f, 72/255f, 51/255f);
    private static readonly Color CardEffectColor = new Color(196/255f, 93/255f, 61/255f);

    void Awake()
    {
        Image image = GetComponent<Image>();
        _tileMat = new Material(image.material);
        image.material = _tileMat;
    }

    public void Init()
    {
        _tileMat.SetColor("_BgColor", EmptyColor);
        _tileMat.SetFloat("_ShowBorder", 0f);
        _tileMat.SetColor("_BorderColor", CardEffectColor);
        _tileMat.SetFloat("_BorderThickness", 0.1f);
        _tileMat.SetFloat("_GradientRange", 0.2f);
    }

    public void SetCenter()
    {
        _tileMat.SetColor("_BgColor", Color.white);
    }

    public void SetPosEffect()
    {
        _tileMat.SetColor("_BgColor", PosEffectColor);
    }

    public void SetCardEffect()
    {
        _tileMat.SetFloat("_ShowBorder", 1f);
    }
}
