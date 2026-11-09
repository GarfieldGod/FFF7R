using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class CardGridCellDebugWindow : EditorWindow
{
    public GridLayoutGroup targetGo;
    // 输入项
    public float panelW = 8f;
    public float panelH = 11f;
    public float GridSize = 2.5f;
    public float spacing = 0.1f;
    public float offsetY = 6.45f;
    // 计算结果
    private bool _isOverFlow;

    [MenuItem("FFF7/卡牌格子计算器")]
    static void OpenWindow()
    {
        GetWindow<CardGridCellDebugWindow>("卡牌格子调试");
    }

    void OnGUI()
    {
        targetGo = EditorGUILayout.ObjectField("GridLayoutGroup", targetGo, typeof(GridLayoutGroup), true) as GridLayoutGroup;
        EditorGUILayout.Space();

        RectTransform rt = null;
        bool valid = false;
        if (targetGo != null)
        {
            rt = targetGo.GetComponent<RectTransform>();
            valid = rt != null;
            if (!valid)
            {
                EditorGUILayout.HelpBox("⚠️ 物体必须同时包含 RectTransform + GridLayoutGroup", MessageType.Error);
            }
        }

        EditorGUILayout.LabelField("【输入参数】", EditorStyles.boldLabel);
        panelW = EditorGUILayout.FloatField("父面板宽度", panelW);
        panelH = EditorGUILayout.FloatField("父面板高度", panelH);
        GridSize = EditorGUILayout.FloatField("Grid整体大小", GridSize);
        spacing = EditorGUILayout.FloatField("Spacing", spacing);
        offsetY = EditorGUILayout.FloatField("网格顶部距离父顶部(offsetY)", offsetY);

        EditorGUILayout.Space(10);

        // 计算公式
        float leftPadding = (panelW - GridSize) / 2;
        float rightPadding = leftPadding;
        float topPadding = offsetY;
        float bottomPadding = panelH - topPadding - GridSize;

        // 判断是否超出父面板
        _isOverFlow = leftPadding < 0 || bottomPadding < 0 || topPadding < 0;
        if (_isOverFlow)
        {
            EditorGUILayout.HelpBox("❌ 阵列尺寸超出父面板！", MessageType.Error);
        }
        else
        {
            EditorGUILayout.HelpBox("✅ 尺寸正常，不会溢出", MessageType.Info);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("阵列水平居中，X自动计算，只使用Y偏移", EditorStyles.miniLabel);
        EditorGUILayout.LabelField($"Left:{leftPadding:F3}  Right:{rightPadding:F3}");
        EditorGUILayout.LabelField($"Top:{topPadding:F3}  Bottom:{bottomPadding:F3}");

        if (valid)
        {
            EditorGUILayout.Space(5);
            if (GUILayout.Button("应用"))
            {
                Undo.RecordObject(targetGo, "修改 GridLayout");
                Undo.RecordObject(rt, "修改 RectTransform Padding");

                float cellSize = (GridSize - 4 * spacing) / 5;
                targetGo.cellSize = Vector2.one * cellSize;
                targetGo.spacing = new Vector2(spacing, spacing);

                // ========== 核心：设置拉伸模式下的 Left / Top / Right / Bottom ==========
                rt.offsetMin = new Vector2(leftPadding, bottomPadding);
                rt.offsetMax = new Vector2(-rightPadding, -topPadding);

                EditorUtility.SetDirty(targetGo);
                EditorUtility.SetDirty(rt);
            }
        }
    }
}
