using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FFF7R;
using System;

[Serializable]
public sealed class CardSelect
{
    public struct TickContext
    {
        public IGameQuery gameQuery;
        public IReadOnlyList<Card> cards;
        public List<Card> canInput;
        public GameObject hit;
        public Camera cam;
        public Quaternion facing;
        public float baseScale;
        public Action RefreshPlayableFx;
        public PlayerSeat seat;
        public CardBoard board;
    }

    public Card Current;
    public SeatAction Action = SeatAction.None;
    public float PreviewHeight = 2.5f;
    public float GrabDepth = 20f;

    private TickContext _context;
    public void Tick(TickContext ctx, out Int2 pos) 
    {
        _context = ctx;
        pos = Int2.Unknown;
        if (Input.GetMouseButtonDown(0))
        {
            SelectUpdate(ctx.board, ctx.cards, ctx.canInput, ctx.hit);
            ActionUpdate(ctx.hit);
        } else
        {
            Action = SeatAction.None;
        }

        if (Current == null) return;

        pos = Update(ctx.gameQuery, ctx.seat, ctx.hit, ctx.cam, ctx.facing, ctx.baseScale);

        if (Input.GetMouseButtonDown(1)) {
            UnSelect();
        }
    }

    public bool Select(Card card)
    {
        UnSelect();

        Current = card;
        Current.Collider.enabled = false;
    
        _context.board.RefreshSelectedCard(_context.seat, Current.HandIndex);
        _context.RefreshPlayableFx?.Invoke();

        return true;
    }

    public void UnSelect()
    {
        _context.board.RefreshSelectedCard(_context.seat, -1);
        if (Current == null) return;

        var card = Current;
        Current.Collider.enabled = true;
        Current = null;
    
        _context.RefreshPlayableFx?.Invoke();
    }

    Int2 Update(IGameQuery gameQuery, PlayerSeat seat, GameObject hit, Camera cam, Quaternion facing, float baseScale)
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

            bool canInput = gameQuery.CanPlace(seat, Current.HandIndex, pos, out var _);
            Current.Fx.SetBanFx(!canInput);
        }

        Current.Motion.TargetPos = worldPos;
        Current.Motion.TargetRot = worldRot;
        Current.Motion.TargetScale = Vector3.one * baseScale;
        return pos;
    }

    public void SelectUpdate(CardBoard cardBoard, IReadOnlyList<Card> cards, List<Card> canInput, GameObject hit)
    {
        if (hit != null && hit.CompareTag("CardInHand") && hit.TryGetComponent(out Card card) && cards.Contains(card)) {
            if (canInput.Contains(card)) {
                Select(card);
            } else {
                card.Fx.PlayBanFxOnce();
            }
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