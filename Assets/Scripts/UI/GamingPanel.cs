using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FFF7R;
using UnityEngine.UI;
using System;

public class GamingPanel : MonoBehaviour
{
    [SerializeField] private TurnBanner turnBanner;
    [SerializeField] private Button nextTurnButton;
    [SerializeField] private PlayerSeat localSeat = PlayerSeat.Player;

    public event Action PassRequested;

    private NextTurnButton next;

    void Awake()
    {
        nextTurnButton.onClick.AddListener(PassTurn);
        next = nextTurnButton.GetComponent<NextTurnButton>();
        next.Hide();
    }

    void OnDestroy()
    {
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
    }
}