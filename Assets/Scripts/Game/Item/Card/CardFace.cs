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

    private static readonly string borderSprites = "CardBorder";
    private static readonly string borderSpritesFull = "CardBorderFull";
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
        GameAssets.Sprites.Request(onBoard ? borderSprites : borderSpritesFull, (s) => {
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

        ApplyPattern(cardData.PosEffect,  c => c.SetPosEffect());
        ApplyPattern(cardData.EffectScope, c => c.SetCardEffect());

        gridCells[2,2].SetCenter();
    }

    void ApplyPattern(GridPattern p, System.Action<GridCellOnCard> mark)
    {
        if (p == null) return;
        for (int y = 0; y < p.Height; y++)
            for (int x = 0; x < p.Width; x++)
                if (p[x, y] != 0) mark(gridCells[x, y]);
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
        gridCells[2,2].SetCenter();
    }
}
