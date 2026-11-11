using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FFF7R;
using UnityEngine.PlayerLoop;

public class BoardGrid : MonoBehaviour
{
    public Int2 Pos;
    public Card currentCard;
    public GridAnim Anim = new GridAnim();

    [Header("卡牌")]
    [SerializeField] float CardHeight = 0.5f; 
    [SerializeField] float cardSize = 0.7f;
    [SerializeField] Card CardPrefab;
    [Header("棋子")]
    [SerializeField] float LevelSpacing = 2f;
    [SerializeField] float LevelHeight = 0.32f;
    [SerializeField] GameObject ballPrefab;
    [SerializeField] int GemSlot = 3;
    [SerializeField] Material PlayerGem;
    [SerializeField] Material RivalGem;

    private readonly Dictionary<GameObject, Material[]> _matCache = new();
    private List<GameObject> _balls = new List<GameObject>();
    private int _baseLevel;

    private void Awake()
    {
        Anim.SetBoardGrid(this);
    }

    public void SyncGrid(CellView cellView)
    {
        switch (cellView.State)
        {
            case CellState.OccupiedPlayer:
                SetGridCard(cellView.CardCode, PlayerSeat.Player, cellView.BaseLevel);
                break;
            case CellState.OccupiedRival:
                SetGridCard(cellView.CardCode, PlayerSeat.Rival, cellView.BaseLevel);
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

    public void SetGridCard(string cardCode, PlayerSeat owner, int baseLevel)
    {
        _baseLevel = baseLevel;
        SetBalls(0, owner);
        if (string.IsNullOrEmpty(cardCode))
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

        if (currentCard.CardCode != cardCode)
            currentCard.SetCardOnBoard(cardCode, owner);
    }

    public void SetGridLevel(int level, PlayerSeat owner)
    {

        if (level > 3) return;

        _baseLevel = 0;
        if (currentCard != null) currentCard.gameObject.SetActive(false);

        SetBalls(level, owner);
    }

    void SetBalls(int level, PlayerSeat owner) { 
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

    public void SetPower(int value) {
        currentCard?.SetPower(value);
        bool on = _baseLevel > 0 && value != _baseLevel;
        bool p = _baseLevel > 0 && value > _baseLevel;
        currentCard?.Fx.SetAuraFx(on, p);
    }

    void SetGridColor(PlayerSeat owner)
    {
        if (owner == PlayerSeat.None) return;

        var gem = owner == PlayerSeat.Player ? PlayerGem : RivalGem;
        if (gem == null) return;

        for (int i = 0; i < _balls.Count; i++)
        {
            var go = _balls[i];
            var mr = go.GetComponent<MeshRenderer>();
            if (mr == null) continue;

            if (!_matCache.TryGetValue(go, out var mats))
                _matCache[go] = mats = mr.sharedMaterials;

            if (GemSlot >= mats.Length) continue;
            if (mats[GemSlot] == gem) continue;

            mats[GemSlot] = gem;
            mr.sharedMaterials = mats;
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
