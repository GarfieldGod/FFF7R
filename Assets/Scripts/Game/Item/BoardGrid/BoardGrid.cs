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
    public Card currentCard;
    public GridAnim Anim = new GridAnim();

    [SerializeField] float cardSize = 0.7f;
    [SerializeField] GameObject ballPrefab;
    [SerializeField] Material[] ballMat;
    [SerializeField] Card CardPrefab;

    List<GameObject> _balls = new List<GameObject>();
    // Start is called before the first frame update

    public void SyncGrid(CellView cellView)
    {
        switch (cellView.State)
        {
            case CellState.OccupiedPlayer:
                SetGridCard(cellView.CardCode, PlayerSeat.Player);
                break;
            case CellState.OccupiedRival:
                SetGridCard(cellView.CardCode, PlayerSeat.Rival);
                break;
            case CellState.Empty:
                SetGridLevel(0, PlayerSeat.None);
                break;
            case CellState.LevelOnePlayer:
                SetGridLevel(1, PlayerSeat.Player);
                break;
            case CellState.LevelTwoPlayer:
                SetGridLevel(2, PlayerSeat.Player);
                break;
            case CellState.LevelThreePlayer:
                SetGridLevel(3, PlayerSeat.Player);
                break;
            case CellState.LevelOneRival:
                SetGridLevel(1, PlayerSeat.Rival);
                break;
            case CellState.LevelTwoRival:
                SetGridLevel(2, PlayerSeat.Rival);
                break;
            case CellState.LevelThreeRival:
                SetGridLevel(3, PlayerSeat.Rival);
                break;
            default:
                break;
        }

        SetPower(cellView.Level);
    }

    public void SetGridCard(string cardCode, PlayerSeat owner)
    {
        if (string.IsNullOrEmpty(cardCode))
        {
            if (currentCard != null) {currentCard.gameObject.SetActive(false);}
            return;
        }
        SetGridLevel(0, owner);

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

        if (currentCard.CardCode != cardCode)
            currentCard.SetCardOnBoard(cardCode, owner);
    }

    public void SetGridLevel(int level, PlayerSeat owner)
    {
        if (level > 3) return;
        if (level > 0) SetGridCard(null, owner);

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

        SetGridColor(owner);
    }

    public void SetPower(int value) => currentCard?.SetPower(value);

    private void SetGridColor(PlayerSeat owner)
    {
        if (owner == PlayerSeat.None) return;
        var mat = ballMat[owner == PlayerSeat.Player ? 0 : 1];
        foreach (var ball in _balls) ball.GetComponent<MeshRenderer>().sharedMaterial = mat;
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
