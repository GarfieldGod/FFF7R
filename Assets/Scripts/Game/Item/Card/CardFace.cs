using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FFF7R;

public class CardFace : MonoBehaviour
{
    [Header("卡面设置")]
    [SerializeField] private Image BackGroundImage;
    [SerializeField] private Image CardImage;
    [SerializeField] private Image BorderImage;
    [Header("名字")]
    [SerializeField] private TextMeshProUGUI NameText;
    [Header("威力")]
    [SerializeField] private PowerNumDisplay PowerText;
    [Header("消耗")]
    [SerializeField] private GridLayoutGroup CostPanel;
    [SerializeField] private Image costCellPrefab;
    [Header("效果范围")]
    [SerializeField] private GridLayoutGroup GridPanel;
    [SerializeField] private GridCellOnCard gridCellPrefab;
    private GridCellOnCard[,] gridCells = null;

    private static readonly string CardFrameBoardNormal = "U_CardGame_CardFrame_Board_Normal_Radius";
    private static readonly string CardFrameBoardSpecial = "U_CardGame_CardFrame_Board_Special_Radius";
    private static readonly string CardFrameHandNormal = "U_CardGame_CardFrame_Hand_Normal_Radius";
    private static readonly string CardFrameHandSpecial = "U_CardGame_CardFrame_Hand_Special_Radius";
    private string _cardCode;

    public void SetCardInHand(CardDefView cardDef, PlayerSeat seat)
    {
        if (_cardCode == cardDef.CardCode) return;

        NameText.enabled = true;
        SetCardData(cardDef);

        SetSprite(cardDef.CardCode, false);
        SetPlayerSeat(seat);
        _cardCode = cardDef.CardCode;
    }

    public void SetCardOnBoard(string cardCode, PlayerSeat seat)
    {
        if (_cardCode == cardCode) return;

        NameText.enabled = false;

        SetSprite(cardCode, true);
        SetPlayerSeat(seat);
        _cardCode = cardCode;
    }

    public void SetPower(int power)
    {
        PowerText.SetNumber(power);
    }

    private void SetPlayerSeat(PlayerSeat seat)
    {
        if (seat == PlayerSeat.Player)
        {
            BackGroundImage.color = new Color(100/255f, 165/255f, 1, 1);
        }
        else if (seat == PlayerSeat.Rival)
        {
            BackGroundImage.color = new Color(1, 50/255f, 50/255f, 1);
        }
    }

    private void SetCardData(CardDefView cardData)
    {
        if (cardData == null)
        {
            Debug.LogError("Card data is null");
            return;
        }

        PowerText.SetNumber(cardData.Level);
        NameText.text = cardData.Name;
        SetCost(cardData.Cost);
        SetScope(cardData);
    }

    private void SetSprite(string cardCode, bool onBoard = false)
    {
        BorderImage.enabled = false;
        GameAssets.Sprites.Request(onBoard ? CardFrameBoardNormal : CardFrameHandNormal, (s) => {
            if (BorderImage != null && s != null) {
                BorderImage.sprite = s;
                BorderImage.enabled = true;
            }
        });

        CardImage.enabled = false;
        GameAssets.Sprites.Request(cardCode, (s) => {
            if (CardImage != null && s != null) {
                CardImage.sprite = s;
                CardImage.enabled = true;
            }
        });
    }

    void SetCost(int cost)
    {
        for (int i = CostPanel.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(CostPanel.transform.GetChild(i).gameObject);
        }

        for (int i = 0; i < cost; i++)
        {
            Instantiate(costCellPrefab, CostPanel.transform);
        }
    }

    void SetScope(CardDefView cardData)
    {
        EnsureGrids();

        ApplyPattern(cardData.PosEffect,  cardData.EffectScope);

        gridCells[2,2].SetState(GridCellOnCard.State.Center);
    }

    void ApplyPattern(GridPattern pe, GridPattern ce)
    {
        if (pe != null)
        {
            for (int y = 0; y < pe.Height; y++)
                for (int x = 0; x < pe.Width; x++)
                {
                    bool hasPosEffect = pe[x, y] != 0;
                    bool hasCardEffect = ce != null && ce[x, y] != 0;
                    GridCellOnCard.State state = GridCellOnCard.State.Empty;
                    if (hasPosEffect && !hasCardEffect) state = GridCellOnCard.State.PosEffect;
                    else if (!hasPosEffect && hasCardEffect) state = GridCellOnCard.State.CardEffect;
                    else if (hasPosEffect && hasCardEffect) state = GridCellOnCard.State.PosEffectAndCardEffect;

                    gridCells[x, y].SetState(state);
                }
        }
    }

    public void EnsureGrids()
    {
        if (gridCells != null)
        {
            foreach(var cell in gridCells)
            {
                cell.Init();
            }
            return;
        }

        gridCells = new GridCellOnCard[5, 5];
        for(int x = 0; x < 5; x++)
        {
            for(int y = 0; y < 5; y++)
            {
                GridCellOnCard cell = Instantiate(gridCellPrefab, GridPanel.transform);
                gridCells[y, x] = cell;
                cell.Init();
            }
        }
        gridCells[2,2].SetState(GridCellOnCard.State.Center);
    }
}
