using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FFF7R;

public class Card : MonoBehaviour
{
    public enum CardStatus
    {
        InHand,
        Selected,
        Previewing,
        InBoard
    }

    public Image CardImage;
    public TextMeshProUGUI NameText;
    public PowerNumDisplay PowerText;
    public GridLayoutGroup CostPanel;
    public GridLayoutGroup GridPanel;

    public Vector3 TargetPos = new Vector3();
    public Quaternion TargetRot = new Quaternion();

    [SerializeField] private GridCellOnCard gridCellPrefab;
    private GridCellOnCard[,] gridCells = new GridCellOnCard[5,5];
    [SerializeField] private Image costCellPrefab;

    public string CardCode;
    public int HandIndex;

    public BoxCollider Collider;
    public CardStatus Status = CardStatus.InBoard;

    void Awake()
    {
        FFF7RCore.Property.LoadChessProperties();
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

    public void Start()
    {
        Collider = GetComponent<BoxCollider>();
    }

    public void SetCard(CardView cardView)
    {
        HandIndex = cardView.HandIndex;
        CardDefView cardData = cardView.Def;
        if (cardData == null)
        {
            Debug.LogError("Card data is null");
            return;
        }
        CardCode = cardData.CardCode;

        NameText.text = cardData.Name;
        PowerText.value = cardData.Level;

        for (int i = CostPanel.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(CostPanel.transform.GetChild(i).gameObject);
        }

        for (int i = 0; i < cardData.Cost; i++)
        {
            Instantiate(costCellPrefab, CostPanel.transform);
        }

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

        PropertyEx.GetCardSpriteAsync(cardData.CardCode, sprite =>
        {
            if (sprite != null)
            {
                CardImage.sprite = sprite;
            }
            else
            {
                Debug.LogWarning($"卡面加载失败: {cardData.CardCode}");
            }
        });
    }

    public void OnHover()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, ~0, QueryTriggerInteraction.Collide)) {
            GameObject hitOne = hit.collider.gameObject;
            // Debug.Log($"命中物体: {hit.collider.gameObject.name} tag:{hit.collider.gameObject.tag}");
            if (hitOne.CompareTag("CardInHand")) {
                // PlyerDoSelect(hitOne);
                Debug.Log("CardInHand Hit");
            }
            if (hitOne.CompareTag("GridInBoard")) {
                // PlayerDoInput(hitOne);
                Debug.Log("GridInBoard Hit");
            }
        }
    }
}
