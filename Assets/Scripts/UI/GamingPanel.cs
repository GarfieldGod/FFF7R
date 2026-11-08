using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FFF7R;
using UnityEngine.UI;
using System;
using TMPro;

public class GamingPanel : MonoBehaviour
{
    [SerializeField] private TurnBanner turnBanner;
    [SerializeField] private Button nextTurnButton;
    [SerializeField] private DetailText CardText;
    [SerializeField] private PlayerSeat localSeat = PlayerSeat.Player;
    [SerializeField] private CardSelector cardSelector;

    public event Action PassRequested;

    private NextTurnButton next;

    void Awake()
    {
        cardSelector.FocusChanged += OnFocusChanged;
        nextTurnButton.onClick.AddListener(PassTurn);
        next = nextTurnButton.GetComponent<NextTurnButton>();
        next.Hide();
        CardText.Show();
    }

    void OnDestroy()
    {
        if (cardSelector != null) cardSelector.FocusChanged -= OnFocusChanged;
        nextTurnButton.onClick.RemoveListener(PassTurn);
    }

    void PassTurn()
    {
        PassRequested?.Invoke();
    }

    public void OnTurnBegin(PlayerSeat seat)
    {
        turnBanner.Show(seat);

        bool myTurn = seat == localSeat;
        next.Active(myTurn);
    }

    public void OnMatchEnd(IGameQuery q){
        next.Hide();
        CardText.Hide();
    }

    public void OnFocusChanged(Card card)
    {
        if (card == null)
        {
            CardText.SetText(string.Empty);
            return;
        }

        CardText.SetText(card.Description);
    }
}