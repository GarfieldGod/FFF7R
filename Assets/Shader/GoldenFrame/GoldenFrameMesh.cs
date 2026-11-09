using UnityEngine;

namespace FFF7R.VFX
{
    /// <summary>
    /// 把 GoldenFrameUI.shader 挂到 3D 网格上（棋盘格子、落子位、地面标记这类）。
    ///
    /// 和 UGUI 版只差一件事：这里没有 RectTransform，shader 不知道该按什么比例铺线宽。
    /// 组件从 Transform 的实际缩放推出尺寸，写进 _QuadW / _QuadH，所以拖大拖小都不用改参数，
    /// 也不吃 UGUI 那套材质实例。
    ///
    /// 用法：挂在带 MeshFilter 的 Quad 上，材质用 GoldenFrame_World.mat（_ZTest = 4 / LEqual）。
    /// 网格由组件自己生成：1×1、中心在原点、躺在 XZ 平面、法线朝 +Y。
    /// 所以直接放在棋盘表面上方 2 mm 就行，**不需要旋转**（Unity 里棋盘是 Y 轴朝上）。
    ///
    /// 编辑器注意：**任何给 sharedMesh 赋值的代码都不能在 OnValidate / OnEnable 里同步执行**，
    /// 详见 ScheduleRefresh 的注释。
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    [AddComponentMenu("FFF7R/VFX/Golden Frame Mesh")]
    public class GoldenFrameMesh : MonoBehaviour
    {
        static readonly int QuadWId = Shader.PropertyToID("_QuadW");
        static readonly int QuadHId = Shader.PropertyToID("_QuadH");

        const string QuadName = "GoldenFrame_Quad";

        static Mesh _quad;                 // 所有实例共用一张 1×1 的 Quad
        MeshFilter _mf;
        MaterialPropertyBlock _mpb;
        Vector3 _lastScale;

        void OnEnable()
        {
#if UNITY_EDITOR
            // 编辑器里 OnEnable 也可能是场景加载 / 预制体导入顺带触发的，
            // 那一刻同样还在 CheckConsistency 阶段，碰 sharedMesh 会报错。统一走延迟。
            if (!Application.isPlaying)
            {
                ScheduleRefresh();
                return;
            }
#endif
            EnsureMesh();
            Apply(true);
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            // 不能在 OnValidate 里碰 sharedMesh：Unity 会立刻发 OnMeshFilterChanged，
            // 而 Awake / CheckConsistency / OnValidate 期间禁止 SendMessage ——
            // 报 "SendMessage cannot be called during Awake, CheckConsistency, or OnValidate"。
            // 挪到下一次编辑器 tick（那时已经不在回调里了），顺带把拖滑块的连续重建合并成一次。
            ScheduleRefresh();
        }

        void ScheduleRefresh()
        {
            UnityEditor.EditorApplication.delayCall -= DelayedRefresh;
            UnityEditor.EditorApplication.delayCall += DelayedRefresh;
        }

        void DelayedRefresh()
        {
            if (this == null) return;      // 拖参数时可能已经把物体删了

            // 预制体资产上不建网格：那个网格是 HideAndDontSave，存不进 .prefab
            // （存下来也只有 m_Mesh: fileID 0），每次导入都白建一份，还会把资产弄脏。
            // 真正实例化到场景里时 OnEnable / Update 会补上。
            if (UnityEditor.PrefabUtility.IsPartOfPrefabAsset(gameObject))
            {
                Apply(true);
                return;
            }

            EnsureMesh();
            Apply(true);
        }
#endif

        void Update()
        {
            // 兜底：域重载后静态 _quad 会被清空，MeshFilter 上的网格也可能被销毁。
            // Update 一定不在禁发消息的阶段里，在这里补建最安全；只补 null，不抢别人塞的网格。
            if (NeedsMesh()) EnsureMesh();

            // 只在尺寸变化时写一次渲染器；ExecuteAlways 下也成立，编辑器里拖缩放能实时跟着变
            if (transform.lossyScale != _lastScale) Apply(false);
        }

        /// <summary>手动重算尺寸。改完父级缩放、或用代码改 transform 之后调一次。</summary>
        public void Apply(bool force)
        {
            var scale = transform.lossyScale;
            if (!force && scale == _lastScale) return;
            _lastScale = scale;

            var r = GetComponent<MeshRenderer>();
            if (r == null) return;

            if (_mpb == null) _mpb = new MaterialPropertyBlock();
            r.GetPropertyBlock(_mpb);
            _mpb.SetFloat(QuadWId, Mathf.Max(1e-4f, Mathf.Abs(scale.x)));
            _mpb.SetFloat(QuadHId, Mathf.Max(1e-4f, Mathf.Abs(scale.z)));
            r.SetPropertyBlock(_mpb);
        }

        bool NeedsMesh()
        {
            if (_mf == null) _mf = GetComponent<MeshFilter>();
            return _mf != null && _mf.sharedMesh == null;
        }

        void EnsureMesh()
        {
            if (_mf == null) _mf = GetComponent<MeshFilter>();
            if (_mf == null) return;

            var existing = _mf.sharedMesh;
            if (existing != null && (existing == _quad || existing.name == QuadName))
            {
                // 域重载会把静态 _quad 清空，但网格对象本身还挂在 MeshFilter 上。
                // 认回来就行，不要再赋一次（赋值 = 又一次 OnMeshFilterChanged）。
                if (_quad == null) _quad = existing;
                return;
            }

            if (_quad == null) _quad = BuildQuad();
            _mf.sharedMesh = _quad;        // 只有这一行会发 OnMeshFilterChanged
        }

        static Mesh BuildQuad()
        {
            var m = new Mesh { name = QuadName, hideFlags = HideFlags.HideAndDontSave };

            // 1×1，中心在原点，躺在 XZ 平面，法线 +Y
            m.vertices = new[]
            {
                new Vector3(-0.5f, 0f, -0.5f),
                new Vector3( 0.5f, 0f, -0.5f),
                new Vector3( 0.5f, 0f,  0.5f),
                new Vector3(-0.5f, 0f,  0.5f),
            };
            m.uv = new[]
            {
                new Vector2(0f, 0f), new Vector2(1f, 0f),
                new Vector2(1f, 1f), new Vector2(0f, 1f),
            };
            // 顶点色必须给白：shader 用顶点色 alpha 当总强度（Blend SrcAlpha One），
            // 缺省通道在部分平台会被填 0，整个框会直接看不见。
            m.colors = new[] { Color.white, Color.white, Color.white, Color.white };
            m.normals = new[] { Vector3.up, Vector3.up, Vector3.up, Vector3.up };
            m.triangles = new[] { 0, 2, 1, 0, 3, 2 };   // 这个绕序才是 +Y 朝上
            m.RecalculateBounds();

            return m;
        }
    }
}
