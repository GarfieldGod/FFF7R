using System;
using System.Collections;
using System.Collections.Generic;
using FFF7R;
using UnityEngine;

public class CardSelector : MonoBehaviour, IGameView
{
    public Card cardPrefab;
    public float CardSelectorLength = -25;
    public float CardSelectorHeight = 1;
    public List<Card> cards = new List<Card>();
    public Card selectedCard;

    private IGameQuery _gameQuery;

    public void Start()
    {
        transform.rotation = ForwardToCam;
    }

    // Start is called before the first frame update
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
        UpdateHand(_gameQuery.Hand(PlayerSeat.Player));
    }

    public void OnEvents(GameEventBatch batch)
    {
        if (_gameQuery == null) return;
        UpdateHand(_gameQuery.Hand(PlayerSeat.Player));
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
                cards[i].SetCard(view);

            cards[i].HandIndex = view.HandIndex;
        }

        LayoutCards();
    }

    public void AddCard(CardView cardView)
    {
        Card card = Instantiate(cardPrefab, transform);
        card.SetCard(cardView);
        cards.Add(card);
    }

    // Update is called once per frame
    void Update()
    {
        Selecting();

        float k = 1f - Mathf.Exp(-12f * Time.deltaTime);
        for (int i = 0; i < cards.Count; i++)
        {
            Card c = cards[i];
            if (c == null || c == selectedCard) continue;
            c.transform.localPosition = Vector3.Lerp(c.transform.localPosition, c.TargetPos, k);
            c.transform.rotation = Quaternion.Slerp(c.transform.rotation, c.TargetRot, k);
        }
    }

    public bool Select(Card card)
    {
        if (!cards.Contains(card)) return false;

        UnSelect();

        selectedCard = card;
        selectedCard.Collider.enabled = false;
        selectedCard.Status = Card.CardStatus.Selected;

        LayoutCards();

        return true;
    }

    public void UnSelect()
    {
        if (selectedCard == null) return;

        selectedCard.Collider.enabled = true;
        selectedCard = null;

        LayoutCards();
    }

    [SerializeField] float _grabDepth = 15;
    Vector3 _vel;
    [SerializeField] Camera cam;

    void Selecting()
    {
        if (selectedCard == null) return;
        if (Input.GetMouseButtonUp(1))
        {
            UnSelect();
            return;
        }

        Vector3 screen = new Vector3(Input.mousePosition.x, Input.mousePosition.y, _grabDepth);
        Vector3 targetPos = cam.ScreenToWorldPoint(screen);
        Quaternion targetRot = ForwardToCam;

        RaycastHit hitHover;
        Ray rayHover = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(rayHover, out hitHover, Mathf.Infinity, ~0, QueryTriggerInteraction.Collide)
            && hitHover.collider.CompareTag("GridInBoard")
        ) {
            targetPos = hitHover.transform.position + new Vector3(0, 5, 0);
            targetRot = hitHover.transform.rotation * Quaternion.Euler(-90, 0, 0);
        }

        selectedCard.transform.position = Vector3.SmoothDamp(
            selectedCard.transform.position, targetPos, ref _vel, 0.05f);
        selectedCard.transform.rotation = Quaternion.Slerp(
            selectedCard.transform.rotation, targetRot, 12f * Time.deltaTime);
    }

    private List<Card> _layoutBuffer = new List<Card>();
    private Quaternion ForwardToCam => cam.transform.rotation * Quaternion.Euler(0, 180f, 0f);

    void LayoutCards()
    {
        if (cards.Count == 0) return;

        _layoutBuffer.Clear();
        foreach (Card card in cards)
        {
            if (card != selectedCard) _layoutBuffer.Add(card);
        }

        float spacingX = CardSelectorLength / _layoutBuffer.Count;
        float spacingZ = CardSelectorHeight / _layoutBuffer.Count;
        for (int i = 0; i < _layoutBuffer.Count; i++)
        {
            float x = spacingX * i;
            float y = 0;
            float z = spacingZ * i;
            _layoutBuffer[i].TargetPos = new Vector3(x, y, z);
            _layoutBuffer[i].TargetRot = ForwardToCam;
        }
    }
}
