using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FFF7R;
using System;

[RequireComponent(typeof(CardSelector))][Serializable]
public class CardLayout
{
    [Header("Layout设置")]
    [SerializeField] private float LayoutWidthMax = -25;
    [SerializeField] private float LayoutWidthMin = -15;
    [SerializeField] private int   WidthStartCount = 5;
    [SerializeField] private int   WidthFullCount  = 10;
    [SerializeField] private float LayoutDepth = 0.5f;
    [SerializeField] private PlayerSeat seat = PlayerSeat.Player;

    [Header("Hover设置")]
    [SerializeField] private float baseScale = 0.7f;
    [SerializeField] private float hoverScale = 0.8f;
    [SerializeField] private float hoverOffset = 0.5f;
    [SerializeField] private int   offsetStartCount = 5;
    [SerializeField] private int   offsetFullCount  = 10;

    [Header("弧形设置")]
    [SerializeField] private float ArcHeight = 0f;                    // 弧高：0=直线，|LayoutWidth|/2=半圆
    [SerializeField][Range(0f, 1f)] private float ArcTilt = 1f;       // 倾斜跟随程度，0=只拱不转

    private List<Card> _layout = new List<Card>();

    public void Refresh(IReadOnlyList<Card> cards, Card sCard, Card hCard, Quaternion facing)
    {
        _layout.Clear();
        foreach (Card card in cards) if (card != sCard) _layout.Add(card);

        int n = _layout.Count;
        if (n == 0) return;

        int hoverIndex = hCard != null ? _layout.IndexOf(hCard) : -1;
        float LayoutWidth = Mathf.Lerp(LayoutWidthMin, LayoutWidthMax, Mathf.InverseLerp(WidthStartCount, WidthFullCount, n));
        float arcTiltTemp = ArcTilt;
        if (n == 2)
        {
            LayoutWidth *= 0.4f;
            arcTiltTemp = ArcTilt * 0.2f;
        }

        // 计算初始间隙
        float WidthStep = LayoutWidth / n;
        float depthStep = LayoutDepth / n;

        // 计算弧高
        float L = Mathf.Abs(LayoutWidth);
        float h = Mathf.Clamp(ArcHeight, 0f, L * 0.5f);
        bool  flat = h <= 0.001f;

        float R = flat ? 0f : (L * L * 0.25f + h * h) / (2f * h);    // 圆的半径
        float d = R - h;                                             // 圆心到弦的距离

        // 计算Hover时挤开距离，根据牌数线性插值
        float t = Mathf.InverseLerp(offsetStartCount, offsetFullCount, n);
        float extra = Mathf.Abs(WidthStep) * (baseScale - hoverScale) * hoverOffset * t;

        for (int i = 0; i < n; i++)
        {
            float u = n > 1 ? 1f - i / (n - 1f) * 2f : 0f;   // 这张牌在整排里的相对位置，范围 -1 到 +1
            float x = u * (L * 0.5f);   // 映射真实位置

            if (hoverIndex >= 0 && i != hoverIndex)
            {
                int diff = i - hoverIndex;
                float falloff = 1f / (1 + Mathf.Abs(diff)); // 距离Hover越远，挤开的距离越少
                x += Mathf.Sign(diff) * extra * falloff;
            }

            float xc = Mathf.Clamp(x, -L * 0.5f, L * 0.5f); // 限制挤开后的位置，避免超出范围
            float y = flat ? 0f : Mathf.Sqrt(Mathf.Max(0f, R * R - xc * xc)) - d; // 算出这张牌该多高
            float tilt = flat ? 0f : Mathf.Asin(xc / R) * Mathf.Rad2Deg; // 算出这张牌该歪多少度

            Transform p = _layout[i].transform.parent;
            _layout[i].Motion.TargetPos = p.position + p.rotation * new Vector3(seat == PlayerSeat.Player ? x : -x, y, depthStep * i);
            _layout[i].Motion.TargetRot = facing  * Quaternion.Euler(0f, 0f, (seat == PlayerSeat.Player ? -tilt : tilt) * arcTiltTemp);
            _layout[i].Motion.TargetScale = Vector3.one * (i == hoverIndex? hoverScale : baseScale);
        }
    }
}