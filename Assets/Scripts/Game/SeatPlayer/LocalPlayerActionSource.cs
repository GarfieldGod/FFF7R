using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FFF7R;

public sealed class LocalPlayerActionSource: ISeatActionSource
{
    public CardSelector cardSelector;
    public LocalPlayerActionSource(CardSelector cardSelector)
    {
        this.cardSelector = cardSelector;
    }

    public void OnResult(CommandResult result)
    {
        if (result.Ok)
        {
            cardSelector.DispenseCard();
        } else {
            Debug.Log($"无法落子：{result.Reason}");
            // cardSelector.NotifyReject();
        }
    }

    public bool TryTake(IGameQuery query, PlayerSeat seat, out SeatAction action)
    {
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
                        action = SeatAction.Place(cardSelector.selectedCard.HandIndex, hitOne.GetComponent<BoardGrid>().Pos);
                        return true;
                    }
                }
            }
        }
        action = SeatAction.None;
        return false;
    }
}
