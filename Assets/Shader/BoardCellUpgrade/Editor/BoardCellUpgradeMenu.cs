using System.IO;
using UnityEditor;
using UnityEngine;

namespace FFF7R.VFX.EditorTools
{
    /// <summary>
    /// 一键创建格子升级特效（部件版）。
    /// 选中一个棋盘格子对象再执行，会自动挂到它下面并对齐格子中心；
    /// 没选中就放在世界原点。
    ///
    /// 生成的结构：
    ///   BoardCellUpgrade              ← BoardCellUpgradeGroup（时间表在这里）
    ///     Fill                        ← CellFXFill    卡牌实心（常量亮面）
    ///     Edge                        ← CellFXEdge    卡牌边框（更亮的那一圈）
    ///     Surge                       ← CellFXSurge   内涌：从卡心扩散到卡边
    ///     Wall                        ← CellFXWall    光墙：涨 → 停 → 消散
    ///     Frame1 / Frame2             ← CellFXFrame   两条垂直边框
    ///     Spread                      ← CellFXSpread  平面扩散（从卡边往外推）
    ///
    /// 形状不再跟格子走：全部按**卡牌** 0.32 × 0.44（8:11）+ 圆角 0.0174，
    /// 见 CellFXShape。所以这个组件挂在格子中心即可，位置就是卡牌中心。
    ///
    /// 菜单：GameObject / VFX / Board Cell Upgrade
    /// </summary>
    public static class BoardCellUpgradeMenu
    {
        const string FillShaderName   = "FFF7/VFX/CellFX/Fill";
        const string EdgeShaderName   = "FFF7/VFX/CellFX/Edge";
        const string SurgeShaderName  = "FFF7/VFX/CellFX/Surge";
        const string WallShaderName   = "FFF7/VFX/CellFX/Wall";
        const string SpreadShaderName = "FFF7/VFX/CellFX/Spread";
        const string FrameShaderName  = "FFF7/VFX/CellFX/Frame";

        static string MatDir
        {
            get
            {
                foreach (var guid in AssetDatabase.FindAssets("CellFXFill t:Shader"))
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    if (Path.GetFileNameWithoutExtension(path) == "CellFXFill")
                        return Path.GetDirectoryName(path).Replace('\\', '/') + "/Materials";
                }
                return "Assets/Shader/BoardCellUpgrade/Materials";   // 兜底，正常走不到
            }
        }

        [MenuItem("GameObject/VFX/Board Cell Upgrade", false, 2003)]
        static void Create(MenuCommand command)
        {
            string missing = null;
            if (Shader.Find(FillShaderName) == null) missing = FillShaderName;
            else if (Shader.Find(EdgeShaderName) == null) missing = EdgeShaderName;
            else if (Shader.Find(SurgeShaderName) == null) missing = SurgeShaderName;
            else if (Shader.Find(WallShaderName) == null) missing = WallShaderName;
            else if (Shader.Find(SpreadShaderName) == null) missing = SpreadShaderName;
            else if (Shader.Find(FrameShaderName) == null) missing = FrameShaderName;

            if (missing != null)
            {
                EditorUtility.DisplayDialog(
                    "找不到 Shader",
                    "没有找到 " + missing + "。\n\n" +
                    "文件在、但整份 shader 解析失败时，Shader.Find 也会返回 null ——\n" +
                    "所以这条报错八成不是「没导入」，而是「导入了但编译不过」。\n\n" +
                    "① Console 里搜 Shader error（完整报错在 %LOCALAPPDATA%\\Unity\\Editor\\Editor.log）\n" +
                    "② 最常见原因：[Header(...)] / [Enum(...)] 的括号里写了中文。\n" +
                    "   attribute 只许 ASCII，中文要放进属性自己的引号标签里。\n" +
                    "③ 不想翻 Console 就跑：python VFX/tools/lint_shaders.py",
                    "知道了");
                return;
            }

            var go = new GameObject("BoardCellUpgrade", typeof(BoardCellUpgradeGroup));
            Undo.RegisterCreatedObjectUndo(go, "Create Board Cell Upgrade");

            var parent = command.context as GameObject;
            if (parent != null && !EditorUtility.IsPersistent(parent))
            {
                go.transform.SetParent(parent.transform, false);
                go.transform.localPosition = Vector3.zero;
                go.transform.localRotation = Quaternion.identity;
                go.transform.localScale = Vector3.one;
            }
            else
            {
                go.transform.position = Vector3.zero;
            }

            var matFill   = GetOrCreateMaterial("CellFXFill",   FillShaderName);
            var matEdge   = GetOrCreateMaterial("CellFXEdge",   EdgeShaderName);
            var matSurge  = GetOrCreateMaterial("CellFXSurge",  SurgeShaderName);
            var matWall   = GetOrCreateMaterial("CellFXWall",   WallShaderName);
            var matSpread = GetOrCreateMaterial("CellFXSpread", SpreadShaderName);
            var matFrame  = GetOrCreateMaterial("CellFXFrame",  FrameShaderName);

            MakePart<CellFXFill>(go, "Fill", matFill);
            MakePart<CellFXEdge>(go, "Edge", matEdge);
            MakePart<CellFXSurge>(go, "Surge", matSurge);
            MakePart<CellFXWall>(go, "Wall", matWall);
            MakePart<CellFXFrame>(go, "Frame1", matFrame);
            MakePart<CellFXFrame>(go, "Frame2", matFrame);
            MakePart<CellFXSpread>(go, "Spread", matSpread);

            var group = go.GetComponent<BoardCellUpgradeGroup>();
            group.ApplyUpgradePreset();
            group.Play(group.level);

            Selection.activeGameObject = go;
            EditorGUIUtility.PingObject(go);

            Debug.Log("[BoardCellUpgrade] 已创建（形状 = 卡牌 " + CellFXShape.CardW + " × " +
                      CellFXShape.CardD + "，圆角 " + CellFXShape.CornerR + "）。\n" +
                      "· 时序：选中 BoardCellUpgrade → 时间表，改「跟随某部件 70%」这类相对触发。\n" +
                      "· 形状与端点：选中各部件 → 尺寸 / 圆角 / 起始宽度 / 末尾宽度 / 起始透明度 等，全在组件上。\n" +
                      "  （这些不进材质 —— Frame1 和 Frame2 共用一个 .mat，放材质上两条框就只能一样粗。）\n" +
                      "· 配色与亮度：在 Materials/ 下的 .mat 上改（整套皮肤共享）。\n" +
                      "· 发几条边框：复制 Frame 对象，时间表里加一行即可。\n" +
                      "· 上线时把 loopPreview 关掉，代码里调 Play(level) 触发。");
        }

        static T MakePart<T>(GameObject root, string name, Material mat) where T : CellFXPart
        {
            var go = new GameObject(name, typeof(T));
            go.transform.SetParent(root.transform, false);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;

            var part = go.GetComponent<T>();
            part.material = mat;
            return part;
        }

        static Material GetOrCreateMaterial(string fileName, string shaderName)
        {
            string path = MatDir + "/" + fileName + ".mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat != null && mat.shader != null && mat.shader.name == shaderName) return mat;

            if (!Directory.Exists(MatDir))
            {
                Directory.CreateDirectory(MatDir);
                AssetDatabase.Refresh();
            }

            mat = new Material(Shader.Find(shaderName)) { name = fileName };
            // 3D 格子必须做深度测试，否则光面会飘到棋子和卡牌前面
            if (mat.HasProperty("_ZTest")) mat.SetFloat("_ZTest", 4f);

            AssetDatabase.CreateAsset(mat, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[BoardCellUpgrade] 材质已生成：" + path);
            return mat;
        }
    }
}
