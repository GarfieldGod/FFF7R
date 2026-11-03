using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FFF7R;

public class Card : MonoBehaviour
{
    public Image CardImage;
    public TextMeshProUGUI NameText;
    public PowerNumDisplay PowerText;
    public GridLayoutGroup CostPanel;
    public GridLayoutGroup GridPanel;

    public Vector3 TargetPos = new Vector3();
    public Quaternion TargetRot = new Quaternion();

    [SerializeField] private GridCellOnCard gridCellPrefab;
    private GridCellOnCard[,] gridCells = null;
    [SerializeField] private Image costCellPrefab;
    [SerializeField] private Image BorderImage;
    [SerializeField] private Image BackGroundImage;
    [SerializeField] private string borderSprites = "CardBorder";
    [SerializeField] private string borderSpritesFull = "CardBorderFull";

    public string CardCode;
    public int HandIndex;

    public BoxCollider Collider;

    void Awake()
    {
        FFF7RCore.Property.LoadChessProperties();
    }

    public void Start()
    {
        Collider = GetComponent<BoxCollider>();
    }

    public void SetPlayerSeat(PlayerSeat seat)
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

    public void SetCardInHand(CardView cardView)
    {
        HandIndex = cardView.HandIndex;
        SetCardData(cardView.Def);
    }

    public void SetCardOnBoard(string cardCode)
    {
        SetSprite(cardCode, true);
    }

    public void SetPower(int power)
    {
        PowerText.value = power;
    }

    private void SetCardData(CardDefView cardData)
    {
        if (cardData == null)
        {
            Debug.LogError("Card data is null");
            return;
        }

        CardCode = cardData.CardCode;
        PowerText.value = cardData.Level;
        NameText.text = cardData.Name;
        SetCost(cardData.Cost);
        SetScope(cardData);

        SetSprite(cardData.CardCode, false);
    }

    private void SetSprite(string cardCode, bool onBoard = false)
    {
        PropertyEx.LoadSpriteAsync(onBoard ? borderSprites : borderSpritesFull,
            sprite =>
            {
                if (BorderImage != null && sprite != null)
                {
                    BorderImage.sprite = sprite;
                }
                else
                {
                    Debug.LogWarning($"卡面加载失败: {cardCode}");
                }
            }
        );

        PropertyEx.GetCardSpriteAsync(cardCode, sprite =>
        {
            if (CardImage != null && sprite != null)
            {
                CardImage.sprite = sprite;
            }
            else
            {
                Debug.LogWarning($"卡面加载失败: {cardCode}");
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

        for(int y = 0; y < 5; y++)
        {
            for(int x = 0; x < 5; x++)
            {
                gridCells[x,y].Init();
            }
        }

        if (cardData.PosEffect != null)
        {
            for(int y = 0; y < cardData.PosEffect.Width; y++)
            {
                for(int x = 0; x < cardData.PosEffect.Height; x++)
                {
                    if(cardData.PosEffect[y, x] != 0)
                    {
                        gridCells[x,y].SetPosEffect();
                    }
                }
            }
        }

        if (cardData.EffectScope != null)
        {
            for(int y = 0; y < cardData.EffectScope.Width; y++)
            {
                for(int x = 0; x < cardData.EffectScope.Height; x++)
                {
                    if (cardData.EffectScope[y, x] != 0)
                    {
                        gridCells[x,y].SetCardEffect();
                    }
                }
            }
        }

        gridCells[2,2].SetCenter();
    }

    public void EnsureGrids()
    {
        if (gridCells != null) return;
        gridCells = new GridCellOnCard[5,5];

        for(int y = 0; y < 5; y++)
        {
            for(int x = 0; x < 5; x++)
            {
                GridCellOnCard cell = Instantiate(gridCellPrefab, GridPanel.transform);
                gridCells[x,y] = cell;
                cell.Init();
            }
        }
        gridCells[2,2].SetCenter();
    }
}
