using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace FFF7R.VFX.EditorTools
{
    /// <summary>
    /// 一键创建金框。两种形态：
    ///   GameObject / UI  / Golden Frame             → UGUI Image + Canvas，界面用
    ///   GameObject / VFX / Golden Frame (Cell Quad) → 3D Quad，贴在棋盘格子这类世界物体上
    /// </summary>
    public static class GoldenFrameMenu
    {
        const string ShaderName = "FFF7/UI/GoldenFrame";
        const string ShaderFile  = "GoldenFrameUI";     // 用来反查 shader 落在工程的哪个目录

        // 棋盘格子：Grid 契约 2.500 × 1.500 / 5×3 格 = 每格 0.500 × 0.500（见 Models/CONTRACT.md）
        const float CellSize = 0.500f;
        const float CellLift = 0.002f;   // 抬 2 mm，避免和棋盘顶面 z-fighting

        // 材质不写死路径：跟着 shader 走，放在它旁边的 Materials/ 下。
        // 这样整个 GoldenFrame 文件夹可以随便放（Assets/Shader/、Assets/VFX/…都行），不用改脚本。
        static string MatDir
        {
            get
            {
                foreach (var guid in AssetDatabase.FindAssets(ShaderFile + " t:Shader"))
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    if (Path.GetFileNameWithoutExtension(path) == ShaderFile)
                        return Path.GetDirectoryName(path).Replace('\\', '/') + "/Materials";
                }
                return "Assets/" + ShaderFile + "/Materials";   // 兜底，正常走不到
            }
        }

        [MenuItem("GameObject/UI/Golden Frame", false, 2000)]
        static void Create(MenuCommand command)
        {
            var shader = Shader.Find(ShaderName);
            if (shader == null)
            {
                EditorUtility.DisplayDialog(
                    "找不到 Shader",
                    "没有找到 " + ShaderName + "。\n\n" +
                    "确认 GoldenFrameUI.shader 已经导入到工程里（不是还在 Assets 外面）。",
                    "知道了");
                return;
            }

            var canvas = ResolveCanvas(command.context as GameObject);
            if (canvas == null)
            {
                EditorUtility.DisplayDialog(
                    "没有 Canvas",
                    "场景里没有 Canvas，先建一个 UI Canvas 再试。",
                    "知道了");
                return;
            }

            var go = new GameObject(
                "GoldenFrame",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(GoldenFrameUI));

            Undo.RegisterCreatedObjectUndo(go, "Create Golden Frame");

            var rt = (RectTransform)go.transform;
            rt.SetParent(canvas.transform, false);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(400f, 520f);
            rt.anchoredPosition = Vector2.zero;
            rt.localScale = Vector3.one;

            var image = go.GetComponent<Image>();
            image.sprite = null;          // 纯程序化，不需要贴图
            image.color = Color.white;
            image.raycastTarget = false;  // 装饰层，不该挡点击
            image.material = GetOrCreateMaterial("GoldenFrame", null);

            Selection.activeGameObject = go;
            EditorGUIUtility.PingObject(go);

            Debug.Log("[GoldenFrame] 已创建。想调参数就选中它，在 Inspector 的材质槽里双击 GoldenFrame.mat 调。" +
                      "尺寸随 RectTransform 走，不用管 _QuadW / _QuadH。");
        }

        /// <summary>
        /// 3D 版：生成一张躺在 XZ 平面的 Quad，挂 GoldenFrameMesh。
        /// 选中某个物体后执行，会自动挂到它下面；没选中就放在世界原点。
        /// </summary>
        [MenuItem("GameObject/VFX/Golden Frame (Cell Quad)", false, 2001)]
        static void CreateCellQuad(MenuCommand command)
        {
            if (Shader.Find(ShaderName) == null)
            {
                EditorUtility.DisplayDialog(
                    "找不到 Shader",
                    "没有找到 " + ShaderName + "。\n\n" +
                    "确认 GoldenFrameUI.shader 已经导入到工程里，并且没有解析错误（看 Console 的 Shader error）。",
                    "知道了");
                return;
            }

            var go = new GameObject(
                "GoldenFrame_Quad",
                typeof(MeshFilter),
                typeof(MeshRenderer),
                typeof(GoldenFrameMesh));

            Undo.RegisterCreatedObjectUndo(go, "Create Golden Frame Quad");

            var parent = command.context as GameObject;
            if (parent != null && !EditorUtility.IsPersistent(parent))
            {
                go.transform.SetParent(parent.transform, false);   // 跟着那个物体走
                go.transform.localPosition = new Vector3(0f, CellLift, 0f);
            }
            else
            {
                go.transform.position = new Vector3(0f, CellLift, 0f);
            }

            go.transform.localRotation = Quaternion.identity;      // 网格本来就躺在 XZ 平面，不用转
            go.transform.localScale = new Vector3(CellSize, 1f, CellSize);

            var mr = go.GetComponent<MeshRenderer>();
            mr.sharedMaterial = GetOrCreateMaterial("GoldenFrame_World", 4f);   // 4 = LEqual，必须做深度测试
            mr.shadowCastingMode = ShadowCastingMode.Off;
            mr.receiveShadows = false;
            mr.lightProbeUsage = LightProbeUsage.Off;
            mr.reflectionProbeUsage = ReflectionProbeUsage.Off;

            go.GetComponent<GoldenFrameMesh>().Apply(true);         // 尺寸立刻写进去，不等下一帧

            Selection.activeGameObject = go;
            EditorGUIUtility.PingObject(go);

            Debug.Log("[GoldenFrame] 3D 框已创建，尺寸 " + CellSize + "×" + CellSize +
                      "。要换格子大小直接改 Transform 的 Scale，组件会自己同步 _QuadW / _QuadH。");
        }

        static Canvas ResolveCanvas(GameObject selected)
        {
            if (selected != null)
            {
                var fromSelection = selected.GetComponentInParent<Canvas>();
                if (fromSelection != null) return fromSelection;
            }
#if UNITY_2022_2_OR_NEWER
            return Object.FindFirstObjectByType<Canvas>();
#else
            return Object.FindObjectOfType<Canvas>();
#endif
        }

        /// <summary>
        /// 取材质，没有就建一个。zTest 传 null = 保持默认（UI 用 8 / Always），
        /// 传 4 = LEqual，给 3D 网格用（必须，否则框会盖住挡在前面的棋子）。
        /// </summary>
        static Material GetOrCreateMaterial(string fileName, float? zTest)
        {
            string path = MatDir + "/" + fileName + ".mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);

            if (mat == null)
            {
                if (!Directory.Exists(MatDir))
                {
                    Directory.CreateDirectory(MatDir);
                    AssetDatabase.Refresh();
                }

                mat = new Material(Shader.Find(ShaderName)) { name = fileName };
                AssetDatabase.CreateAsset(mat, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                Debug.Log("[GoldenFrame] 材质已生成：" + path);
            }

            if (zTest.HasValue && mat.HasProperty("_ZTest"))
            {
                mat.SetFloat("_ZTest", zTest.Value);
                EditorUtility.SetDirty(mat);
            }

            return mat;
        }
    }
}
