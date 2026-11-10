using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace FFF7R.VFX.EditorTools
{
    /// <summary>
    /// 一键创建卡牌辉芒（卡牌边缘往外持续发散的光芒）。
    ///
    /// 菜单：GameObject / VFX / Card Aura
    ///   选中一张卡牌再执行 → 挂到它下面；没选中 → 放到世界原点。
    ///
    /// 生成的就是一个对象：
    ///   CardAura   ← MeshFilter + MeshRenderer + CardAuraFX（网格按尺寸建，躺在 XZ 平面）
    ///
    /// 尺寸烘在网格里、Transform 恒为 (1,1,1)，不用手调；位置只抬 Y（卡牌顶面 0.020 → 抬到 0.0265）。
    /// </summary>
    public static class CardAuraMenu
    {
        const string ShaderName = "FFF7/VFX/CardAura";
        const string ShaderFile = "CardAuraFX";     // 用来反查 shader 落在工程哪个目录

        // 卡牌顶面 0.020（见 Models/CONTRACT.md / CellFXShape），再抬 6.5 mm 避免和卡面 z-fighting
        const float AuraLift = 0.0265f;

        // 材质不写死路径：跟着 shader 走，放它旁边的 Materials/ 下。
        // 这样整个 CardAura 文件夹可以随便放（Assets/Shader/、Assets/VFX/…），不用改脚本。
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
                return "Assets/Shader/CardAura/Materials";     // 兜底，正常走不到
            }
        }

        [MenuItem("GameObject/VFX/Card Aura", false, 2002)]
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
                "CardAura",
                typeof(MeshFilter),
                typeof(MeshRenderer),
                typeof(CardAuraFX));

            Undo.RegisterCreatedObjectUndo(go, "Create Card Aura");

            var parent = command.context as GameObject;
            if (parent != null && !EditorUtility.IsPersistent(parent))
            {
                go.transform.SetParent(parent.transform, false);   // 跟着那张卡走
                go.transform.localPosition = new Vector3(0f, AuraLift, 0f);
            }
            else
            {
                go.transform.position = new Vector3(0f, AuraLift, 0f);
            }

            go.transform.localRotation = Quaternion.identity;      // 网格本来就躺在 XZ 平面，不用转

            var mr = go.GetComponent<MeshRenderer>();
            mr.sharedMaterial = GetOrCreateMaterial();
            mr.shadowCastingMode = ShadowCastingMode.Off;
            mr.receiveShadows = false;
            mr.lightProbeUsage = LightProbeUsage.Off;
            mr.reflectionProbeUsage = ReflectionProbeUsage.Off;

            var aura = go.GetComponent<CardAuraFX>();
            aura.Refit();                                          // 尺寸 / 参数立刻写进去，不等下一帧

            Selection.activeGameObject = go;
            EditorGUIUtility.PingObject(go);

            Debug.Log("[CardAura] 已创建，尺寸 " + aura.cardSize.x + " × " + aura.cardSize.y +
                      "（quad 比卡牌每边大 " + (aura.padding * 100f).ToString("0") + "%）。\n" +
                      "· Y 已抬到 " + AuraLift + "（相对父级）。如果父级原点在卡牌中面而不是棋盘面，" +
                      "把它改成 0.0085。\n" +
                      "· 换个颜色：双击 Materials/CardAura.mat 改「光芒颜色 / 光芒亮度」。\n" +
                      "· 条数 / 扰动 / 发散距离 / 转速：选中 CardAura，都在 CardAuraFX 组件上（不进材质）。\n" +
                      "· 开关：勾 on，或代码里 aura.SetOn(true)；0.17 s 平滑淡入。");
        }

        static Material GetOrCreateMaterial()
        {
            string path = MatDir + "/CardAura.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat != null && mat.shader != null && mat.shader.name == ShaderName) return mat;

            if (!Directory.Exists(MatDir))
            {
                Directory.CreateDirectory(MatDir);
                AssetDatabase.Refresh();
            }

            mat = new Material(Shader.Find(ShaderName)) { name = "CardAura" };
            // 3D 卡牌必须做深度测试，否则光芒会飘到棋子和卡牌前面
            if (mat.HasProperty("_ZTest")) mat.SetFloat("_ZTest", 4f);

            AssetDatabase.CreateAsset(mat, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[CardAura] 材质已生成：" + path);
            return mat;
        }
    }
}
