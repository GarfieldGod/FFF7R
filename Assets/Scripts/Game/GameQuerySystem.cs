using System;
using System.Collections.Generic;
using FFF7R;
using FFF7R.Adapter;
using UnityEngine;

public class GameQuerySystem : MonoBehaviour
{
    public GameBootstrap gameBootstrap;
    private IGameQuery gameQuery;
    public CardSelector PlayerCardSelector;
    public CardBoard CardBoard;
    // Start is called before the first frame update

    private Action<HandSnapshot> UpdateHand;
    private Action<BoardSnapshot> UpdateBoard;

    void Awake()
    {
        gameBootstrap.SessionCreated += OnSessionCreated;
    }

    void OnSessionCreated(IGameSession gameSession)
    {
        gameQuery = gameSession;
        PlayerCardSelector.UpdateHand(gameQuery.Hand(PlayerSeat.Player));
        CardBoard.UpdateBoard(gameQuery.Board);
    }
}