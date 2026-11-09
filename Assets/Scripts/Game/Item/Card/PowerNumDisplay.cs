using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PowerNumDisplay : MonoBehaviour
{
    [SerializeField] private int testValue = -1;
    [Header("0~9 精灵数组，顺序 0,1,2,3...9")]
    public Sprite[] digitSprites;
    [Header("数字的大小")]
    public List<float> digitSizeList = new List<float>() { 55, 45, 35};
    public float digitHeightRate = 1.6f;
    public float digitOffset = 0;

    [Header("数字之间的间距")]
    public float spacing = 0;

    private List<Image> _spawnedDigits = new List<Image>();


    private void Update()
    {
        if (testValue != -1) SetNumber(testValue);
    }

    public void SetNumber(int value)
    {
        string numStr = value.ToString();
        ClearOldDigits();

        float digitSize = 55;
        if (numStr.Length > digitSizeList.Count)
        {
            digitSize = digitSizeList[digitSizeList.Count - 1];
        } else
        {
            digitSize = digitSizeList[numStr.Length - 1];
        }

        float startX = -(numStr.Length - 1) * (digitSize + spacing) / 2f + digitOffset;
        for (int i = 0; i < numStr.Length; i++)
        {
            char c = numStr[i];
            int idx = c - '0';
            if (idx < 0 || idx >= digitSprites.Length) continue;

            // 生成Image
            GameObject go = new GameObject($"Digit_{c}");
            go.transform.SetParent(transform, false);
            Image img = go.AddComponent<Image>();
            img.sprite = digitSprites[idx];
            img.rectTransform.sizeDelta = new Vector2(digitSize, digitSize * digitHeightRate);
            img.rectTransform.anchoredPosition = new Vector2( startX + i*(digitSize + spacing), 0 );
            _spawnedDigits.Add(img);
        }
    }

    private void ClearOldDigits()
    {
        foreach(var img in _spawnedDigits)
        {
            Destroy(img.gameObject);
        }
        _spawnedDigits.Clear();
    }
}
