using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FFF7R;
using UnityEngine.PlayerLoop;

public class BoardGrid : MonoBehaviour
{
    public Int2 Pos;
    public float LevelSpacing = 2f;
    public float LevelHeight = 1f;
    public float CardHeight = 0.5f; 

    [SerializeField] float cardSize = 0.7f;
    [SerializeField] GameObject ballPrefab;
    [SerializeField] Material[] ballMat;
    [SerializeField] Card CardPrefab;
    public Card currentCard;
    List<GameObject> _balls = new List<GameObject>();
    // Start is called before the first frame update

    public void UpdateBoardGrid(CellView cellView)
    {
        SetGridState(cellView.State);
        SetGridCard(cellView);
        if (currentCard != null) currentCard.SetPower(cellView.Level);
    }

    public void SetGridCard(CellView cellView)
    {
        if (cellView.CardCode == null)
        {
            if (currentCard != null) {currentCard.gameObject.SetActive(false);}
            return;
        }

        if (currentCard == null)
        {
            currentCard = Instantiate(CardPrefab, transform, false);
            currentCard.transform.localPosition = new Vector3(0, CardHeight, 0);
            currentCard.transform.localRotation = Quaternion.Euler(-90, 0, 0);
        } else
        {
            currentCard.gameObject.SetActive(true);
        }

        currentCard.transform.localScale = Vector3.one * cardSize;

        if (currentCard.CardCode != cellView.CardCode)
            currentCard.SetCardOnBoard(cellView.CardCode, cellView.Owner);
    }

    public void SetGridState(CellState cellState)
    {
        switch (cellState)
        {
            case CellState.OccupiedPlayer:
            case CellState.OccupiedRival:
            case CellState.Empty:
                SetGridLevel(0);
                break;
            case CellState.LevelOnePlayer:
                SetGridLevel(1);
                SetGridColor(0);
                break;
            case CellState.LevelTwoPlayer:
                SetGridLevel(2);
                SetGridColor(0);
                break;
            case CellState.LevelThreePlayer:
                SetGridLevel(3);
                SetGridColor(0);
                break;
            case CellState.LevelOneRival:
                SetGridLevel(1);
                SetGridColor(1);
                break;
            case CellState.LevelTwoRival:
                SetGridLevel(2);
                SetGridColor(1);
                break;
            case CellState.LevelThreeRival:
                SetGridLevel(3);
                SetGridColor(1);
                break;
            default:
                break;
        }
    }

    public void SetGridColor(int i)
    {
        var mat = ballMat[i];
        foreach (var ball in _balls) ball.GetComponent<MeshRenderer>().sharedMaterial = mat;
    }

    public void SetGridLevel(int level)
    {
        if (level > 3) return;

        while (_balls.Count < level)
        {
            var go = Instantiate(ballPrefab, transform, false);
            go.name = $"Ball_{_balls.Count + 1}";
            _balls.Add(go);
        }

        for (int i = 0; i < _balls.Count; i++)
        {
            if (i >= level) { _balls[i].SetActive(false); continue; }
            _balls[i].SetActive(true);
            _balls[i].transform.localPosition = BollPos(level, i);
        }
    }

    static readonly Vector3[][] Layout =
    {
        new Vector3[0],
        new[] { new Vector3(0, 1, 0) },
        new[] { new Vector3(-0.5f, 1, 0), new Vector3(0.5f, 1, 0) },
        new[] { new Vector3(-0.5f, 1, 0.5f), new Vector3(0.5f, 1, 0.5f), new Vector3(0, 1, -0.5f) },
    };

    private Vector3 BollPos(int level, int index)
    {
        var n = Layout[level][index];
        return new Vector3(n.x * LevelSpacing, n.y * LevelHeight, n.z * LevelSpacing);
    }
}
