using System;
using UnityEngine;
using UnityEngine.EventSystems;
using FFF7R;

public class CardUI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] CardFace face;

    public CardDefView Def { get; private set; }
    public event Action<CardUI> Clicked;

    public void Start()
    {
        CardDefView def = App.Cards.Get("Card001");
        Bind(def, PlayerSeat.Player);
    }

    public void Bind(CardDefView def, PlayerSeat seat)
    {
        if (def == null) { Debug.LogError("[CardUI] def 为空"); return; }
        Def = def;
        face.SetCardInHand(def, seat);
    }

    public void OnPointerClick(PointerEventData e) => Clicked?.Invoke(this);
}
