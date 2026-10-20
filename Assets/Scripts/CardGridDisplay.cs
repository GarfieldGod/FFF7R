using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardGridDisplay : MonoBehaviour
{
    [SerializeField] private Image cellPrefab;
    private Image[,] grid = new Image[5,5];
    private GridLayoutGroup gridLayout;

    void Awake()
    {
        gridLayout = GetComponent<GridLayoutGroup>();

        for(int y = 0; y < 5; y++)
        {
            for(int x=0;x < 5;x++)
            {
                Image cell = Instantiate(cellPrefab, transform);
                grid[x,y] = cell;
                cell.color = Color.black;
            }
        }
    }

    private void SetCellColor(int x, int y, Color color)
    {
        if(x >=0 && x<5 && y>=0 && y<5)
        {
            grid[x,y].color = color;
        }
    }
}
