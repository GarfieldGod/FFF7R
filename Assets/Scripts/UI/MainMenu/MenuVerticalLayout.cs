using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class MenuVerticalLayout : MonoBehaviour
{
    [SerializeField] float startY = -130f;
    [SerializeField] float spacing = 20f;

    void OnValidate() => Apply();
    void Awake() => Apply();

    public void Apply()
    {
        float y = startY;
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            if (!(transform.GetChild(i) is RectTransform rt)) continue;

            rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, y);
            y += rt.sizeDelta.y + spacing;
        }
    }
}