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

    public void UpdateHand(HandSnapshot hand)
    {
        while (cards.Count > hand.Count)
        {
            Destroy(cards[^1].gameObject);
            cards.RemoveAt(cards.Count - 1);
        }

        for (int i = 0; i < hand.Count; i++)
        {
            CardView view = hand.Cards[i];

            if (i >= cards.Count)
                AddCard(view);
            else if (cards[i].CardCode != view.CardCode)
            {
                cards[i].SetCardInHand(view, seat, faceUp);
            }

            cards[i].HandIndex = view.HandIndex;
        }
    }

    public void AddCard(CardView cardView)
    {
        Card card = Instantiate(cardPrefab, transform);
        card.SetCardInHand(cardView, seat, faceUp);
        cards.Add(card);
    }

    public void TickSelect()
    {
        if (seat == PlayerSeat.Player) {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            GameObject hitObj = null;
            if (Physics.Raycast(ray, out var hit, Mathf.Infinity, ~0, QueryTriggerInteraction.Collide)) {
                hitObj = hit.collider.gameObject;
            }
            select.Tick(cards, hitObj, cam, ForwardToCam);
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
        if (select.Current == null) return;
        Card card = select.Current;
        select.UnSelect();
        card.transform.position = DispenseCardSrc.position;
    }
}
