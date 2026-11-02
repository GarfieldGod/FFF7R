using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FFF7R;

public class CardBoard : MonoBehaviour, IGameView
{
    private IGameQuery _gameQuery;
    private List<List<BoardGrid>> _boardCells = new List<List<BoardGrid>>();
    // Start is called before the first frame update
    public void Bind(IGameQuery gameQuery)
    {
        _gameQuery = gameQuery;
        Resync();
    }

    public void Unbind()
    {
        _gameQuery = null;
    }

    public void Resync()
    {
        if (_gameQuery == null) return;
        UpdateBoard(_gameQuery.Board);
    }

    public void OnEvents(GameEventBatch batch)
    {
        if (_gameQuery == null) return;
        UpdateBoard(_gameQuery.Board);
    }

    public void UpdateBoard(BoardSnapshot board)
    {
        // board.Width 是横向格数（棋盘 y 轴），board.Height 是纵向格数（棋盘 x 轴）。
        // 行对应高度方向（x）、列对应宽度方向（y），所以行列边界直接对应 Height / Width。
        if (_boardCells.Count != board.Height || (_boardCells.Count > 0 && _boardCells[0].Count != board.Width))
        {
            GenerateBoardGirds(board);
        }

        // Debug.Log($"UpdateBoardGrid Board Size: {board.Width}, {board.Height}");
        // Debug.Log($"UpdateBoardGrid UI Size: {_boardCells.Count}");
        for (int x = 0; x < board.Height; x++)
        {
            for (int y = 0; y < board.Width; y++)
            {
                CellView cellView = board[x, y];
                // Debug.Log($"UpdateBoardGrid CellView {x}, {y} Pos: {cellView.Pos} State: {cellView.State}");
                _boardCells[x][y].UpdateBoardGrid(cellView);
            }
        }
    }

    [Header("棋盘配置")]
    [SerializeField] private GameObject BoardModel;
    [SerializeField] private GameObject BoardGridGroup;
    [SerializeField] private GameObject BoardGridPrefab;
    [SerializeField] private float colliderHeight = 0.1f;

    void GenerateBoardGirds(BoardSnapshot board)
    {
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
                GameObject go = Instantiate(BoardGridPrefab, parent);

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
