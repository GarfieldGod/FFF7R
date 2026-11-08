using System;
using System.Collections;
using System.Collections.Generic;
using FFF7R;
using UnityEngine;

public class CardSelector : MonoBehaviour, IGameView
{
    public List<Card> cards = new List<Card>();
    public SeatAction Action => select.Action;

    [Header("设置")]
    [SerializeField] private CardBoard board;
    [SerializeField] private Card cardPrefab;
    [SerializeField] private Transform DispenseCardSrc;
    [SerializeField] private Camera cam;
    [SerializeField] private CardLayout layout = new CardLayout();
    [SerializeField] private CardSelect select = new CardSelect();
    private CardHover hover = new CardHover();

    public PlayerSeat seat = PlayerSeat.Player;
    public bool faceUp = true;

    private IGameQuery _gameQuery;
    private Quaternion ForwardToCam;

    private int  _lastHand = -1;
    private Int2 _lastPos  = Int2.Unknown;
    private bool _hadPreview;

    public void Start()
    {
        ForwardToCam = seat == PlayerSeat.Player ? 
            cam.transform.rotation * Quaternion.Euler(0, 180f, 0f) :
            cam.transform.rotation * Quaternion.Euler(0, 180f, 180f);

        transform.rotation = ForwardToCam;
    }

    public void Bind(IGameQuery gameQuery)
    {
        _gameQuery = gameQuery;
        Resync();
    }

    public void Unbind()
    {
        _gameQuery = null;
    }

    public void Resync()
    {
        if (_gameQuery == null) return;
        UpdateHand(_gameQuery.Hand(seat));
    }

    public void OnEvents(GameEventBatch batch)
    {
        if (_gameQuery == null) return;
        UpdateHand(_gameQuery.Hand(seat));
    }

    Card _consumed;

    public void UpdateHand(HandSnapshot hand)
    {
        var pool = new List<CardView>(hand.Cards);
        var keep = new List<Card>(cards.Count);

        foreach (Card c in cards)
        {
            int idx = pool.FindIndex(v => v.CardCode == c.CardCode);
            if (idx < 0)
            {
                // 快照里没有 → 它已经不在这手牌里（打出了 / 被效果去掉）
                if (c == select.Current) select.Current = null;   // 别走 UnSelect，它会碰 Collider
                Destroy(c.gameObject);
                continue;
            }

            c.HandIndex = pool[idx].HandIndex;
            pool.RemoveAt(idx);
            keep.Add(c);
        }

        cards.Clear();
        cards.AddRange(keep);

        foreach (CardView v in pool) AddCard(v);
    }

    public void AddCard(CardView cardView)
    {
        Card card = Instantiate(cardPrefab, transform);
        card.SetCardInHand(cardView, seat, faceUp);
        card.transform.position = DispenseCardSrc.position;
        cards.Add(card);
    }

    public void TickSelect()
    {
        if (seat == PlayerSeat.Player && _gameQuery.State.IsGaming) {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            GameObject hitObj = null;
            if (Physics.Raycast(ray, out var hit, Mathf.Infinity, ~0, QueryTriggerInteraction.Collide)) {
                hitObj = hit.collider.gameObject;
            }
            select.Tick(cards, hitObj, cam, ForwardToCam, out var pos);

            bool want = select.Current != null && pos != Int2.Unknown;
            if (!want)
            {
                if (_hadPreview) { board.ClearPreview(); _hadPreview = false; }
                _lastHand = -1; _lastPos = Int2.Unknown;
                return;
            }

            bool changed = !_hadPreview || pos != _lastPos || select.Current.HandIndex != _lastHand;
            if (!changed) return;                       // 目标没变，什么都不做

            var snapshot = _gameQuery.Preview(seat, select.Current.HandIndex, pos);
            _lastHand = select.Current.HandIndex;
            _lastPos  = pos;

            if (snapshot == null)
            {
                _hadPreview = false;
                board.ClearPreview();
                return;
            }
            _hadPreview = true;
            board.ShowPreview(_gameQuery.Preview(seat, _lastHand, pos));
        }
    }

    public SeatAction TakeAction()
    {
        var a = select.Action;
        select.Action = SeatAction.None;
        return a;
    }

    void LateUpdate()
    {
        if (!_gameQuery.State.IsGaming) return;
        if (seat == PlayerSeat.Player) {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            GameObject hitObj = null;
            if (Physics.Raycast(ray, out var hit, Mathf.Infinity, ~0, QueryTriggerInteraction.Collide)) {
                hitObj = hit.collider.gameObject;
            }
            // select.Tick(cards, hitObj, cam, ForwardToCam);
            hover.Tick(cards, hitObj, cam, select.Current != null);
        }

        layout.Refresh(cards, select.Current, hover.Current, ForwardToCam);
    }

    public void OnInputSuccess()
    {
        if (select.Current != null) select.UnSelect();
    }
}