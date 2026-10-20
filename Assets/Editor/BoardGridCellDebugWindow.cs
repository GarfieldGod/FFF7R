using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class BoardGridCellDebugWindow : EditorWindow
{
    [Header("棋盘配置")]
    private GameObject BoardModel;
    private GameObject BoardGridGroup;
    private GameObject BoardGridPrefab;

    private int colCount = 5;
    private int rowCount = 3;
    private float colliderHeight = 0.1f;    // 碰撞盒Y向厚度，自定义
    private bool useTrigger = true;

    [MenuItem("FFF7/棋盘格子计算器")]
    static void OpenWindow()
    {
        GetWindow<BoardGridCellDebugWindow>("棋盘格子调试");
    }

    void OnGUI()
    {
        BoardGridGroup = EditorGUILayout.ObjectField("棋盘格子组", BoardGridGroup, typeof(GameObject), true) as GameObject;
        BoardModel = EditorGUILayout.ObjectField("目标棋盘模型", BoardModel, typeof(GameObject), true) as GameObject;
        BoardGridPrefab = EditorGUILayout.ObjectField("格子Prefab", BoardGridPrefab, typeof(GameObject), true) as GameObject;

        EditorGUILayout.Space();
        colCount = EditorGUILayout.IntField("列数", colCount);
        rowCount = EditorGUILayout.IntField("行数", rowCount);
        colliderHeight = EditorGUILayout.FloatField("碰撞盒高度(Y)", colliderHeight);
        useTrigger = EditorGUILayout.Toggle("碰撞盒设为Trigger", useTrigger);

        EditorGUILayout.Space();
        if (GUILayout.Button("生成逻辑格子"))
        {
            if (BoardModel == null || BoardGridGroup == null || BoardGridPrefab == null)
            {
                EditorUtility.DisplayDialog("提示", "请把三个对象都填好！", "OK");
                return;
            }
            if (colCount <= 0 || rowCount <= 0)
            {
                EditorUtility.DisplayDialog("提示", "行列必须大于0", "OK");
                return;
            }

            MeshFilter mf = BoardModel.GetComponent<MeshFilter>();
            if (mf == null || mf.sharedMesh == null)
            {
                EditorUtility.DisplayDialog("错误", "棋盘模型必须带 MeshFilter + Mesh", "OK");
                return;
            }

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

            Transform parent = groupT;
            Undo.RecordObject(parent, "清空棋盘旧格子");
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Transform child = parent.GetChild(i);
                DestroyImmediate(child.gameObject);
            }

            // 核心公式：总可用长度 = boardW，分给 colCount 个格子 + (colCount-1) 个间隙
            float cellW = boardW  / colCount;
            float cellD = boardH  / rowCount;

            float halfBoardW = boardW / 2f;
            float halfBoardH = boardH / 2f;

            // 阵列起始位置：第一个格子的中心
            float startX = -halfBoardW + cellW * 0.5f;
            float startZ = -halfBoardH + cellD * 0.5f;

            for (int x = 0; x < colCount; x++)
            {
                for (int y = 0; y < rowCount; y++)
                {
                    float posX = startX + x * cellW;
                    float posZ = startZ + y * cellD;

                    GameObject go = PrefabUtility.InstantiatePrefab(BoardGridPrefab, parent) as GameObject;
                    Undo.RegisterCreatedObjectUndo(go, "生成棋盘格子");

                    go.transform.localPosition = new Vector3(posX, 0, posZ);
                    go.name = $"BoardGrid_{x}_{y}";

                    // 传入格子尺寸、自定义高度
                    SetCustomBoxCollider(go, cellW, cellD, colliderHeight, useTrigger);
                }
            }
            Debug.Log($"棋盘格子生成完成：{colCount}列 × {rowCount}行 | cellW={cellW:F4}");
        }
    }

    /// <summary>
    /// 自定义设置碰撞盒，XY使用格子尺寸，Y高度自定义
    /// </summary>
    private void SetCustomBoxCollider(GameObject go, float sizeX, float sizeZ, float heightY, bool isTrigger)
    {
        BoxCollider col = go.GetComponent<BoxCollider>();
        if (col == null)
        {
            col = go.AddComponent<BoxCollider>();
        }
        // 中心：Y方向抬高一半厚度，保证上下对称
        col.center = new Vector3(0, 0, 0);
        col.size = new Vector3(sizeX, heightY, sizeZ);
        col.isTrigger = isTrigger;
    }
}
