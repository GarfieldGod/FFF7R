using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Card : MonoBehaviour
{
    public Image CardImage;
    public TextMeshProUGUI NameText;
    public PowerNumDisplay PowerText;
    public GridLayoutGroup CostPanel;
    public GridLayoutGroup GridPanel;

    [SerializeField] private GridCellOnCard gridCellPrefab;
    private GridCellOnCard[,] gridCells = new GridCellOnCard[5,5];
    [SerializeField] private Image costCellPrefab;

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

    public void SetCard(string cardCode)
    {
        SetCard(FFF7RCore.Property.GetChessProperty(cardCode));
    }

    public void SetCard(FFF7RCore.ChessProperty cardData)
    {
        if (cardData == null)
        {
            Debug.LogError("Card data is null");
            return;
        }

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
            for(int y = 0; y < cardData.PosEffect.Count; y++)
            {
                for(int x = 0; x < cardData.PosEffect[0].Count; x++)
                {
                    if(cardData.PosEffect[y][x] != 0)
                    {
                        gridCells[x,y].SetPosEffect();
                    }
                }
            }
        }

        if (cardData.CardEffect != null && cardData.CardEffect.Scope != null)
        {
            for(int y = 0; y < cardData.CardEffect.Scope.Count; y++)
            {
                for(int x = 0; x < cardData.CardEffect.Scope[0].Count; x++)
                {
                    if (cardData.CardEffect.Scope[y][x] != 0)
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

    // Start is called before the first frame update
    void Start()
    {
        // SetCard("Card001");
    }

    // Update is called once per frame
    void Update()
    {
        
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
