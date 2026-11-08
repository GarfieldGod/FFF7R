using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public sealed class CardReDispense
{
    public List<Card> Selected = new List<Card>();
    [SerializeField] Transform AbList;
    [SerializeField] float LayoutWidthSpace;
    [SerializeField] float LayoutDepth;

    public void Tick(IReadOnlyList<Card> cards, GameObject hit, Quaternion facing, Card hCard, float baseScale, float hoverScale) {
        if (Input.GetMouseButtonDown(0))
        {
            if (hit != null && hit.CompareTag("CardInHand") && hit.TryGetComponent(out Card card) && cards.Contains(card)) {
                if (Selected.Contains(card)) {
                    Selected.Remove(card);
                } else {
                    Selected.Add(card);
                }
            }
        }

        Layout(facing, hCard, baseScale, hoverScale);
    }

    public void Layout(Quaternion facing, Card hCard, float baseScale, float hoverScale)
    {
        int n = Selected.Count;
        if (n == 0) return;
        var depthStep = LayoutDepth / n;
        var LayoutWidth = LayoutWidthSpace * n;

        int hoverIndex = hCard != null && Selected.Contains(hCard) ? Selected.IndexOf(hCard) : -1;

        for (int i = 0; i < n; i++)
        {
            float u = n > 1 ? 1f - i / (n - 1f) * 2f : 0f;   // 这张牌在整排里的相对位置，范围 -1 到 +1
            float x = u * (Mathf.Abs(LayoutWidth) * 0.5f);   // 映射真实位置

            Selected[i].Motion.TargetPos = AbList.position + AbList.rotation * new Vector3(x, 0, depthStep * i);
            Selected[i].Motion.TargetRot = facing;
            Selected[i].Motion.TargetScale = Vector3.one * (i == hoverIndex? hoverScale : baseScale);;
        }
    }
}