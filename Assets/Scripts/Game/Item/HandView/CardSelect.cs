using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FFF7R;
using System;

[Serializable]
public sealed class CardSelect
{
    public Card Current;
    public SeatAction Action = SeatAction.None;
    public float PreviewHeight = 2.5f;
    public float GrabDepth = 20f;
    public void Tick(IReadOnlyList<Card> cards, GameObject hit, Camera cam, Quaternion facing, out Int2 pos, float baseScale) {
        pos = Int2.Unknown;
        if (Input.GetMouseButtonDown(0))
        {
            SelectUpdate(cards, hit);
            ActionUpdate(hit);   
        } else
        {
            Action = SeatAction.None;
        }

        if (Current == null) return;

        pos = Update(hit, cam, facing, baseScale);

        if (Input.GetMouseButtonDown(1)) UnSelect();
    }

    public bool Select(Card card)
    {
        UnSelect();

        Current = card;
        Current.Collider.enabled = false;

        return true;
    }

    public void UnSelect()
    {
        if (Current == null) return;

        var card = Current;
        Current.Collider.enabled = true;
        Current = null;
    }

    Int2 Update(GameObject hit, Camera cam, Quaternion facing, float baseScale)
    {
        Int2 pos = Int2.Unknown;
        if (Current == null) return pos;

        Vector3 screen = new Vector3(Input.mousePosition.x, Input.mousePosition.y, GrabDepth);
        Vector3 worldPos = cam.ScreenToWorldPoint(screen);
        Quaternion worldRot = facing;

        if (hit != null && hit.CompareTag("GridInBoard")) {
            worldPos = hit.transform.position + new Vector3(0, PreviewHeight, 0);
            worldRot = hit.transform.rotation * Quaternion.Euler(-90, 0, 0);
            pos = hit.GetComponent<BoardGrid>().Pos;
        }

        Current.Motion.TargetPos = worldPos;
        Current.Motion.TargetRot = worldRot;
        Current.Motion.TargetScale = Vector3.one * baseScale;
        return pos;
    }

    public void SelectUpdate(IReadOnlyList<Card> cards, GameObject hit)
    {
        if (hit != null && hit.CompareTag("CardInHand") && hit.TryGetComponent(out Card card) && cards.Contains(card)) {
            Select(card);
        }
    }

    public void ActionUpdate(GameObject hit)
    {
        if (hit != null && hit.CompareTag("GridInBoard") && Current != null) {
            Action = SeatAction.Place(Current.HandIndex, hit.GetComponent<BoardGrid>().Pos);
        } else {
            Action = SeatAction.None;
        }
    }
}