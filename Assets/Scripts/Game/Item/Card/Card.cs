using UnityEngine;
using UnityEngine.UI;
using FFF7R;

[RequireComponent(typeof(BoxCollider))]
public class Card : MonoBehaviour
{
    [SerializeField] private CardFace Face;
    [SerializeField] public CardMotion Motion;
    [SerializeField] private Image CardMask;
    [SerializeField] public CardFx Fx = new CardFx();

    public string CardCode;
    public int HandIndex;
    public string Description;
    public BoxCollider Collider;

    public CardMotion.CStatus Status
    {
        get => Motion.Status;
        set => Motion.Status = value;
    }

    public void Awake()
    {
        Collider = GetComponent<BoxCollider>();
        Collider.enabled = false;
        Fx.Hide(true);
    }

    public void OnEnable()
    {
        Fx.Hide(true);
    }

    public void SetPower(int value) => Face.SetPower(value);

    public void SetCardInHand(CardView cardView, PlayerSeat seat, bool face)
    {
        HandIndex = cardView.HandIndex;
        CardCode = cardView.Def.CardCode;
        Description = cardView.Def.Description;
        Motion.MotionEnabled = true;
        Collider.enabled = true;

        if (!face) SetCardInvisible();
        else Face.SetCardInHand(cardView.Def, seat);
    }

    public void SetCardOnBoard(string cardCode, PlayerSeat seat)
    {
        CardCode = cardCode;
        Motion.MotionEnabled = false;
        Collider.enabled = false;
        Face.SetCardOnBoard(cardCode, seat);
    }

    public void SetCardVisible()
    {
        CardMask.gameObject.SetActive(false);
        Face.gameObject.SetActive(true);
    }

    public void SetCardInvisible()
    {
        CardCode = "__back";
        Face.gameObject.SetActive(false);
        CardMask.gameObject.SetActive(true);
    }
}
