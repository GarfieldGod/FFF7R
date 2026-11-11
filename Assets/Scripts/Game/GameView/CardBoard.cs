using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FFF7R;
using UnityEngine.SocialPlatforms.Impl;
using System;
using System.Linq;

public class CardBoard : MonoBehaviour, IGameView
{
    [Header("分数格配置")]
    [SerializeField] private GameObject ScoreGridGroup;
    [SerializeField] private GameObject ScoreGridPrefab;
    [SerializeField] private float ScoreGridOffset = 5f;
    [SerializeField] private float ScoreGridHeight = 0.5f;

    [Header("棋盘格配置")]
    [SerializeField] private GameObject BoardModel;
    [SerializeField] private GameObject BoardGridGroup;
    [SerializeField] private GameObject BoardGridPrefab;
    [SerializeField] private float colliderHeight = 0.1f;

    private IGameQuery _gameQuery;
    private List<List<BoardGrid>> _boardCells;
    private List<ScoreGrid> _playerScores;
    private List<ScoreGrid> _rivalScores;
    private bool _previewing;
    private BoardSnapshot _previewBase;

    public AnimQueue Anim;

    public BoardGrid this[Int2 p]
    {
        get { return _boardCells[p.X][p.Y]; }
    }

    // Start is called before the first frame update
    public void Bind(IGameQuery gameQuery)
    {
        Anim.Clear();
        _gameQuery = gameQuery;
        Resync();
    }

    public void Unbind()
    {
        Anim.Clear();
        _gameQuery = null;
    }

    public void Resync()
    {
        ApplySnapshot(_gameQuery.Board);
    }

    public void ApplySnapshot(BoardSnapshot s)
    {
        if (s == null) return;
        _previewing = false;
        _previewBase = null;
        UpdateBoard(s);
    }

    public void OnEvents(GameEventBatch batch)
    {
        if (_gameQuery == null) return;
        ClearPreview();
        BoardSnapshot final = _gameQuery.Board;

        // ① 预扫描：Grid 按落子位置分桶。用 Queue，防止同 Pos 出现两次
        var gridByPos = new Dictionary<Int2, Queue<IAnimStep>>();
        foreach (var e in batch.Events)
        {
            if (e is GridLevelsChangedEvent g)
            {
                if (!gridByPos.TryGetValue(g.Pos, out var q))
                    gridByPos[g.Pos] = q = new Queue<IAnimStep>();
                q.Enqueue(AnimDirector.EventToAnimStep(this, g, TranslateEvent(g)));
            }
        }

        foreach (var e in batch.Events)
        {
            if (e is ChessPlacedEvent p)
            {
                var placed = AnimDirector.EventToAnimStep(this, p, TranslateEvent(p));
                if (placed != null) Anim.Enqueue(placed);

                if (gridByPos.TryGetValue(p.Pos, out var q))
                    while (q.Count > 0) Anim.Enqueue(q.Dequeue());
            }
            else if (e is GridLevelsChangedEvent g)
            {
                continue;
            }
            else
            {
                var s = AnimDirector.EventToAnimStep(this, e, TranslateEvent(e));
                if (s != null) Anim.Enqueue(s);
            }
        }

        foreach (var q in gridByPos.Values)                   // ③ 兜底 flush
            while (q.Count > 0) Anim.Enqueue(q.Dequeue());

        SyncScoreGrids(final);
        Anim.Enqueue(new ActionStep(() => ApplySnapshot(final)));
    }

    public Action TranslateEvent(GameEvent e)
    {
        switch (e) {
            case ChessPlacedEvent p:
                return () => { this[p.Pos].SetGridCard(p.CardCode, p.Owner, p.BasePower); this[p.Pos].SetPower(p.CurrentPower); };
            case ChessBuffedEvent b:
                return () => { this[b.Pos].SetPower(b.AfterPower); };
            case ChessDeadEvent d:
                return () => { this[d.Pos].SetGridLevel(1, d.Owner); };
            case GridLevelsChangedEvent g:
                return () => { foreach (var c in g.Cells) {int level = c.After; this[c.Pos].SetGridLevel(c.After, c.Owner);} };
            case BuffsExpiredEvent x:
                return () => { foreach (var c in x.Cells) this[c.Pos].SetPower(c.After); };
            case TurnEndedEvent t:
            case GameEndedEvent end:
            default:
                return null;
        }
    }

    public void ShowPreview(BoardSnapshot snapshot)
    {
        if (snapshot == null) return;
        if (!_previewing) _previewBase = _gameQuery.Board;
        _previewing = true;
        UpdateBoard(snapshot);
    }

    public void ClearPreview()
    {
        if (!_previewing) return;
        _previewing = false;

        BoardSnapshot back = _previewBase;
        _previewBase = null;
        if (back != null) UpdateBoard(back);
        else if (_gameQuery != null) UpdateBoard(_gameQuery.Board);   // 兜底
    }

    public void RefreshSelectedCard(PlayerSeat seat, int handIndex){
        if (_gameQuery == null) return;
        Debug.Log($"RefreshSelectedCard");

        foreach (var line in _boardCells)
        {
            foreach(var cell in line)
            {
                bool active = handIndex > 0 ? _gameQuery.CanPlace(seat, handIndex, cell.Pos, out var _) : false;
                cell.Anim.SetCardPlayableVFX(active);
            }
        }
    }

    private void UpdateBoard(BoardSnapshot board)
    {
        if (board == null) return;
        SyncGrids(board);
        SyncScoreGrids(board);
    }

    // 分数格子 更新与创建
    private void SyncScoreGrids(BoardSnapshot board)
    {
        if (_playerScores == null || _playerScores.Count != board.Height || 
            _rivalScores == null || _rivalScores.Count != board.Height)
        {
            GenerateScoreGrids(board);
        }

        for (int x = 0; x < board.Height; x++)
        {
            _playerScores[x].SetScore(board.PlayerScoreList[x]);
            _rivalScores[x].SetScore(board.RivalScoreList[x]);

            _playerScores[x].Invalid = board.PlayerScoreList[x] <= board.RivalScoreList[x] ? true : false;
            _rivalScores[x].Invalid = board.PlayerScoreList[x] >= board.RivalScoreList[x] ? true : false;
        }
    }

    private void GenerateScoreGrids(BoardSnapshot board)
    {
        Transform parent = ScoreGridGroup.transform;
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Transform child = parent.GetChild(i);
            child.SetParent(null);
            Destroy(child.gameObject);
        }

        _playerScores = new List<ScoreGrid>();
        _rivalScores = new List<ScoreGrid>();
        for (int i = 0; i < board.Height; i++)
        {
            var goP = Instantiate(ScoreGridPrefab, ScoreGridGroup.transform, false);
            var gridP = goP.GetComponent<ScoreGrid>();
            _playerScores.Add(gridP);
            goP.transform.localPosition = _boardCells[i][0].transform.localPosition + new Vector3(ScoreGridOffset, ScoreGridHeight, 0);

            var goR = Instantiate(ScoreGridPrefab, ScoreGridGroup.transform, false);
            var gridR = goR.GetComponent<ScoreGrid>();
            _rivalScores.Add(gridR);
            goR.transform.localPosition = _boardCells[i][^1].transform.localPosition + new Vector3(-ScoreGridOffset, ScoreGridHeight, 0);
        }
    }

    private void SyncGrids(BoardSnapshot board)
    {
        if (board == null) return;
        if (_boardCells == null || _boardCells.Count != board.Height || (_boardCells.Count > 0 && _boardCells[0].Count != board.Width))
        {
            GenerateBoardGirds(board);
        }

        for (int x = 0; x < board.Height; x++)
        {
            for (int y = 0; y < board.Width; y++)
            {
                CellView cellView = board[x, y];
                _boardCells[x][y].SyncGrid(cellView);
            }
        }
    }

    // 棋盘格子 更新与创建
    private void GenerateBoardGirds(BoardSnapshot board)
    {
        Debug.Log($"[Rebuild BoardGirds] {Time.frameCount} {board.Width}x{board.Height} cells={_boardCells?.Count}");
        int rowCount= board.Height; 
        int colCount= board.Width;
        MeshFilter mf = BoardModel.GetComponent<MeshFilter>();

        Transform modelT = BoardModel.transform;
        Transform groupT = BoardGridGroup.transform;

        Bounds meshLocalBounds = mf.sharedMesh.bounds;
        Vector3 worldMin = modelT.TransformPoint(meshLocalBounds.min);
        Vector3 worldMax = modelT.TransformPoint(meshLocalBounds.max);

        Vector3 gMin = groupT.InverseTransformPoint(worldMin);
        Vector3 gMax = groupT.InverseTransformPoint(worldMax);
        Bounds localBoardBounds = new Bounds();
        localBoardBounds.SetMinMax(gMin, gMax);

        float boardW = Mathf.Abs(localBoardBounds.size.x);
        float boardH = Mathf.Abs(localBoardBounds.size.z);

        // 清空子对象
        Transform parent = groupT;
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Transform child = parent.GetChild(i);
            Destroy(child.gameObject);
        }

        // 核心公式：总可用长度 = boardW，分给 colCount 个格子 + (colCount-1) 个间隙
        float cellW = boardW  / colCount;
        float cellD = boardH  / rowCount;

        float halfBoardW = boardW / 2f;
        float halfBoardH = boardH / 2f;

        // 阵列起始位置：第一个格子的中心
        float startX = -halfBoardW + cellW * 0.5f;
        float startZ = -halfBoardH + cellD * 0.5f;

        _boardCells = new List<List<BoardGrid>>();
        for (int x = 0; x < rowCount; x++)
        {
            List<BoardGrid> rowCells = new List<BoardGrid>();
            for (int y = 0; y < colCount; y++)
            {
                GameObject go = Instantiate(BoardGridPrefab, parent, false);

                var boardGrid = go.GetComponent<BoardGrid>();
                boardGrid.Pos = board[x, y].Pos;
                rowCells.Add(boardGrid);

                SetCustomBoxCollider(go, cellW, cellD, colliderHeight);
                go.name = $"BoardGrid_{x}_{y}";

                float posX = startX + y * cellW;
                float posZ = startZ + x * cellD;
                go.transform.localPosition = new Vector3(-posX, 0, posZ);
            }
            _boardCells.Add(rowCells);
        }

        Debug.Log($"棋盘格子生成完成：{colCount}列 × {rowCount}行 | cellW={cellW:F4}");
    }

    private void SetCustomBoxCollider(GameObject go, float sizeX, float sizeZ, float heightY)
    {
        BoxCollider col = go.GetComponent<BoxCollider>();
        if (col == null)
        {
            col = go.AddComponent<BoxCollider>();
        }
        col.center = new Vector3(0, 0, 0);
        col.size = new Vector3(sizeX, heightY, sizeZ);
        col.isTrigger = true;
    }
}
