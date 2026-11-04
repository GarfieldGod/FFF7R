using System;
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
        CardCode = cardCode;
        NameText.enabled = false;
        SetSprite(cardCode, true);
    }

    public void SetPower(int power)
    {
        PowerText.SetNumber(power);
    }

    private void SetCardData(CardDefView cardData)
    {
        if (cardData == null)
        {
            Debug.LogError("Card data is null");
            return;
        }

        CardCode = cardData.CardCode;
        PowerText.SetNumber(cardData.Level);
        NameText.text = cardData.Name;
        SetCost(cardData.Cost);
        SetScope(cardData);

        SetSprite(cardData.CardCode, false);
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
