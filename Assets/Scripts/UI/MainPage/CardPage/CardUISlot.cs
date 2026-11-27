using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardUISlot : MonoBehaviour
{
    [SerializeField] private CardUI cardUI;
    [SerializeField] private TMP_Text currentNumText;
    [SerializeField] private TMP_Text maxNumText;
    [SerializeField] private Image Mask;

    public string CardCode { get; private set; }

    public event Action<string> Clicked;

    private readonly static Color TextColor = new Color(176/255f, 139/255f, 83/255f, 1);

    void Awake()   => cardUI.Clicked += HandleCardClicked;
    void OnDestroy() => cardUI.Clicked -= HandleCardClicked;

    void HandleCardClicked(string code)
    {
        if (string.IsNullOrEmpty(code)) return;
        Clicked?.Invoke(code);
    }

    public void Bind(string cardCode)
    {
        CardCode = cardCode;
        cardUI.Bind(cardCode);
        // maxNumText.text = DeckRules.MaxCardCount(cardCode).ToString();
    }

    public void SetNum(int currentNum, int maxNum)
    {
        if (currentNum == 0) {
            currentNumText.color = Color.gray;
            UIUtils.SetAlpha(Mask, 1);
        }
        else {
            currentNumText.color = TextColor;
            UIUtils.SetAlpha(Mask, 0);
        }

        currentNumText.text = currentNum.ToString();
        maxNumText.text = maxNum.ToString();
    }
}
