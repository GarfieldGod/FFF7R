using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace FFF7R.VFX.EditorTools
{
    /// <summary>
    /// 一键创建卡牌禁止（卡面中央的禁止标 + 轻微压暗）。
    ///
    /// 菜单：GameObject / VFX / Card Ban
    ///   选中一张卡牌再执行 → 挂到它下面；没选中 → 放到世界原点。
    ///
    /// 生成的就是一个对象：
    ///   CardBan   ← MeshFilter + MeshRenderer + CardBanFX（网格按尺寸建，躺在 XZ 平面）
    ///
    /// 尺寸烘在网格里、Transform 恒为 (1,1,1)，不用手调；位置只抬 Y（卡牌顶面 0.020 → 抬到 0.0265）。
    /// </summary>
    public static class CardBanMenu
    {
        const string ShaderName = "FFF7/VFX/CardBan";
        const string ShaderFile = "CardBanFX";     // 用来反查 shader 落在工程哪个目录

        // 卡牌顶面 0.020（见 Models/CONTRACT.md / CellFXShape），再抬 6.5 mm 避免和卡面 z-fighting。
        // 和 CardAura 用同一个高度：两者叠在一张卡上时不要互相打架。
        const float BanLift = 0.0265f;

        // 材质不写死路径：跟着 shader 走，放它旁边的 Materials/ 下。
        // 这样整个 CardBan 文件夹可以随便放（Assets/Shader/、Assets/VFX/…），不用改脚本。
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
                return "Assets/Shader/CardBan/Materials";     // 兜底，正常走不到
            }
        }

        [MenuItem("GameObject/VFX/Card Ban", false, 2003)]
        static void Create(MenuCommand command)
        {
            if (Shader.Find(ShaderName) == null)
            {
                EditorUtility.DisplayDialog(
                    "找不到 Shader",
                    "没有找到 " + ShaderName + "。\n\n" +
                    "文件在、但整份 shader 解析失败时，Shader.Find 也会返回 null ——\n" +
                    "所以这条报错八成不是「没导入」，而是「导入了但编译不过」。\n\n" +
                    "① Console 里搜 Shader error（完整报错在 %LOCALAPPDATA%\\Unity\\Editor\\Editor.log）\n" +
                    "② 最常见原因：[Header(...)] / [Enum(...)] 的括号里写了中文。\n" +
                    "   attribute 只许 ASCII，中文要放进属性自己的引号标签里。\n" +
                    "③ 不想翻 Console 就跑：python VFX/tools/lint_shaders.py",
                    "知道了");
                return;
            }

            var go = new GameObject(
                "CardBan",
                typeof(MeshFilter),
                typeof(MeshRenderer),
                typeof(CardBanFX));

            Undo.RegisterCreatedObjectUndo(go, "Create Card Ban");

            var parent = command.context as GameObject;
            if (parent != null && !EditorUtility.IsPersistent(parent))
            {
                go.transform.SetParent(parent.transform, false);   // 跟着那张卡走
                go.transform.localPosition = new Vector3(0f, BanLift, 0f);
            }
            else
            {
                go.transform.position = new Vector3(0f, BanLift, 0f);
            }

            go.transform.localRotation = Quaternion.identity;      // 网格本来就躺在 XZ 平面，不用转

            var mr = go.GetComponent<MeshRenderer>();
            mr.sharedMaterial = GetOrCreateMaterial();
            mr.shadowCastingMode = ShadowCastingMode.Off;
            mr.receiveShadows = false;
            mr.lightProbeUsage = LightProbeUsage.Off;
            mr.reflectionProbeUsage = ReflectionProbeUsage.Off;

            var ban = go.GetComponent<CardBanFX>();
            ban.Refit();                                           // 尺寸 / 参数立刻写进去，不等下一帧

            Selection.activeGameObject = go;
            EditorGUIUtility.PingObject(go);

            Debug.Log("[CardBan] 已创建，尺寸 " + ban.cardSize.x + " × " + ban.cardSize.y + "。\n" +
                      "· Y 已抬到 " + BanLift + "（相对父级）。如果父级原点在卡牌中面而不是棋盘面，" +
                      "把它改成 0.0085。\n" +
                      "· 换颜色：双击 Materials/CardBan.mat 改「符号颜色 / 符号亮度」。\n" +
                      "· 符号大小 / 粗细 / 斜杠角度 / 不透明度 / 压暗 / 呼吸：选中 CardBan，都在 CardBanFX 上（不进材质）。\n" +
                      "· 开关：勾 on，或代码里 ban.SetOn(true)；0.17 s 平滑淡入。");
        }

        static Material GetOrCreateMaterial()
        {
            string path = MatDir + "/CardBan.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat != null && mat.shader != null && mat.shader.name == ShaderName) return mat;

            if (!Directory.Exists(MatDir))
            {
                Directory.CreateDirectory(MatDir);
                AssetDatabase.Refresh();
            }

            mat = new Material(Shader.Find(ShaderName)) { name = "CardBan" };
            // 3D 卡牌必须做深度测试，否则符号会飘到棋子和别的卡牌前面
            if (mat.HasProperty("_ZTest")) mat.SetFloat("_ZTest", 4f);

            AssetDatabase.CreateAsset(mat, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[CardBan] 材质已生成：" + path);
            return mat;
        }
    }
}
