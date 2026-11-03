using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FFF7R;

public sealed class LocalSeatPlayer: ISeatPlayer
{
    public PlayerSeat Seat { get; }
    public CardSelector cardSelector;
    public LocalSeatPlayer(PlayerSeat seat, CardSelector cardSelector)
    {
        Seat = seat;
        this.cardSelector = cardSelector;
    }

    public void Tick(IGameQuery query, ISeatCommands commands, ISeatInputSource input)
    {
        PlayerClick(commands);
    }

    void PlayerClick(ISeatCommands c) {
        if (Input.GetMouseButtonDown(0)) {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, ~0, QueryTriggerInteraction.Collide)) {
                GameObject hitOne = hit.collider.gameObject;
                if (hitOne.CompareTag("CardInHand")) {
                    if (hitOne.TryGetComponent(out Card card)) {
                        cardSelector.Select(card);
                    }
                }
                if (hitOne.CompareTag("GridInBoard")) {
                    if (cardSelector.selectedCard != null)
                    {
                        c.Place(Seat, cardSelector.selectedCard.HandIndex, hitOne.GetComponent<BoardGrid>().Pos);
                    }
                }
            }
        }
    }
}
