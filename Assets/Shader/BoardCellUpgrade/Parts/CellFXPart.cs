using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace FFF7R.VFX
{
    /// <summary>
    /// 格子特效部件 · 基类。
    ///
    /// 约定只有一条：**部件只吃一个 0..1 进度**（SetProgress）。
    /// 它不知道自己在什么时候被播、被谁播、正放还是倒放，也不认识别的部件。
    ///
    /// 形状统一按**卡牌**来：0.32 × 0.44（8:11）+ 圆角 0.0174，
    /// 不再是格子那个 0.3636 × 0.5 的矩形。四个部件共用同一套轮廓，
    /// 所以墙根、边框、扩散环、平面亮带在同一条线上，看起来是一件东西。
    ///
    /// 平面部件（Fill / Edge / Spread）在片元里用圆角矩形的**有符号距离**，
    /// 圆角和比例是解析算出来的，不是靠网格细分 —— 放大多少倍都不会出棱。
    /// 立体部件（Wall / Frame）才吃 `cornerSegments`。
    /// </summary>
    [ExecuteAlways]
    public abstract class CellFXPart : MonoBehaviour
    {
        [Header("外观")]
        [Tooltip("色调，乘在材质颜色上。一组特效给同一个 tint 就能一次性换配色")]
        public Color tint = Color.white;
        [Tooltip("亮度，乘在材质亮度上")]
        [Range(0f, 4f)] public float intensity = 1f;
        [Tooltip("等级 0-3，只影响亮度，不改形状")]
        [Range(0, 3)] public int level = 1;

        [Header("形状（卡牌 8:11 + 圆角）")]
        [Tooltip("卡牌尺寸（米）。0.32 × 0.44 就是卡牌本身的尺寸")]
        public Vector2 cardSize = new Vector2(CellFXShape.CardW, CellFXShape.CardD);
        [Tooltip("圆角半径（米）。0.0174 就是卡牌几何上的那个圆角")]
        public float cornerRadius = CellFXShape.CornerR;
        [Tooltip("每个圆角分几段。墙和垂直边框按它建网格；平面部件走 SDF，不吃这个")]
        [Range(1, 16)] public int cornerSegments = 6;

        [Header("时间")]
        [Tooltip("这个部件从进度 0 走到 1 用多久（秒）。Group 的时间表读这里")]
        public float duration = 0.60f;
        [Tooltip("进度曲线，只改形状不改时间。默认线性")]
        public AnimationCurve curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        [Header("材质")]
        public Material material;

        // ---- shader 公共属性 ----------------------------------------
        protected static readonly int TintId      = Shader.PropertyToID("_Tint");
        protected static readonly int IntensityId = Shader.PropertyToID("_Intensity");
        protected static readonly int LevelId     = Shader.PropertyToID("_Level");
        protected static readonly int UnitId      = Shader.PropertyToID("_Unit");
        protected static readonly int ProgressId  = Shader.PropertyToID("_Progress");
        protected static readonly int HalfWId     = Shader.PropertyToID("_HalfW");
        protected static readonly int HalfDId     = Shader.PropertyToID("_HalfD");
        protected static readonly int RadiusId    = Shader.PropertyToID("_Radius");

        protected MeshRenderer _mr;
        protected MeshFilter _mf;
        protected Mesh _mesh;
        protected MaterialPropertyBlock _mpb;
        Material _runtimeMat;                        // 只在找不到 .mat 资产时才有（不落盘，不写回字段）
        protected Vector2 _card = new Vector2(CellFXShape.CardW, CellFXShape.CardD);
        protected float _unit = CellFXShape.CardW;

        float _p = -1f;
        bool _built;
        Color _external = Color.white;

        /// <summary>子类给出自己用的 shader 名（FFF7/VFX/CellFX/...）。</summary>
        protected abstract string ShaderName { get; }

        public MeshRenderer R { get { return _mr; } }

        protected virtual void OnEnable()
        {
            Ensure(true);
            if (_p < 0f) _p = 0f;
            Apply(_p, true);
        }

        protected virtual void OnValidate()
        {
            // 还没建过就别在 OnValidate 里 AddComponent（预制体实例化时会报错）
            if (_mr == null) return;

#if UNITY_EDITOR
            // 不能在 OnValidate 里碰 sharedMesh：Unity 会立刻发 OnMeshFilterChanged，
            // 而 Awake / CheckConsistency / OnValidate 期间禁止 SendMessage ——
            // 报 "SendMessage cannot be called during Awake, CheckConsistency, or OnValidate"。
            // 挪到下一次编辑器 tick（那时已经不在回调里了），顺带把拖滑块的连续重建合并成一次。
            UnityEditor.EditorApplication.delayCall -= DelayedRefresh;
            UnityEditor.EditorApplication.delayCall += DelayedRefresh;
#else
            Ensure(true);
            Apply(_p < 0f ? 0f : _p, true);
#endif
        }

#if UNITY_EDITOR
        void DelayedRefresh()
        {
            if (this == null) return;   // 拖参数时可能已被删掉
            Ensure(true);
            Apply(_p < 0f ? 0f : _p, true);
        }
#endif

        protected virtual void OnDisable()
        {
            // 组件关掉后不会再 Apply，renderer 还挂着、材质冻在最后一帧 —— 一并关掉
            if (_mr != null) _mr.enabled = false;
        }

        protected virtual void OnDestroy()
        {
            if (_mesh != null)
            {
                if (Application.isPlaying) Destroy(_mesh);
                else DestroyImmediate(_mesh);
                _mesh = null;
            }
            if (_runtimeMat != null)                    // 临时材质要自己收，否则每次重建都漏一个
            {
                if (Application.isPlaying) Destroy(_runtimeMat);
                else DestroyImmediate(_runtimeMat);
                _runtimeMat = null;
            }
        }

        /// <summary>唯一入口：0..1 进度。谁给、什么时候给、正放倒放，都不关这里的事。</summary>
        public void SetProgress(float raw)
        {
            float p = Mathf.Clamp01(raw);
            if (curve != null && curve.length > 1) p = Mathf.Clamp01(curve.Evaluate(p));
            Apply(p, false);
        }

        /// <summary>组级的统一配色，乘在本部件的 tint 上。</summary>
        public void SetExternalTint(Color c)
        {
            // if (_external == c) return;
            _external = c;
            Apply(Mathf.Max(_p, 0f), true);
        }

        void Apply(float p, bool force)
        {
            if (!force && Mathf.Abs(p - _p) < 1e-4f) return;
            _p = p;

            if (_mr == null) return;
            _mr.enabled = p > 1e-4f;      // 进度归零 = 彻底关掉，不留在场景里
            // if (p <= 1e-4f) return;

            if (_mpb == null) _mpb = new MaterialPropertyBlock();
            _mr.GetPropertyBlock(_mpb);
            _mpb.SetColor(TintId, tint * _external);
            _mpb.SetFloat(IntensityId, intensity);
            _mpb.SetFloat(LevelId, level);
            _mpb.SetFloat(UnitId, Mathf.Max(_unit, 1e-4f));
            _mpb.SetFloat(ProgressId, p);
            // 卡牌轮廓：平面部件靠这三个值在片元里算有符号距离
            _mpb.SetFloat(HalfWId, _card.x * 0.5f);
            _mpb.SetFloat(HalfDId, _card.y * 0.5f);
            _mpb.SetFloat(RadiusId, Mathf.Max(cornerRadius, 1e-5f));
            Push(p, _mpb);
            _mr.SetPropertyBlock(_mpb);
        }

        /// <summary>把本部件自己的那几个参数写进去。公共的已经写好了。</summary>
        protected abstract void Push(float p, MaterialPropertyBlock b);

        /// <summary>建网格。返回前调用 SetGeometry 提交。</summary>
        protected abstract void BuildGeometry(Vector2 card);

        // ---- 构建 ----------------------------------------------------

        protected void Ensure(bool force)
        {
            _mf = GetOrAdd<MeshFilter>(gameObject);
            _mr = GetOrAdd<MeshRenderer>(gameObject);

            var mat = ResolveMaterial();
            if (mat != null && _mr.sharedMaterial != mat) _mr.sharedMaterial = mat;
            ConfigureRenderer(_mr);

            if (_built && !force) return;
            _built = true;

            _card = new Vector2(Mathf.Max(cardSize.x, 1e-3f), Mathf.Max(cardSize.y, 1e-3f));
            _unit = Mathf.Min(_card.x, _card.y);

            if (_mesh == null)
                _mesh = new Mesh { name = name + "Mesh", hideFlags = HideFlags.HideAndDontSave };
            _mesh.Clear();
            BuildGeometry(_card);

            // 顶点着色器会把几何搬走（垂直边框要往上飞），包围盒给足，否则会被视锥剔除掉
            _mesh.bounds = new Bounds(new Vector3(0f, _unit * 1.2f, 0f),
                                      new Vector3(_card.x * 4f, _unit * 4f, _card.y * 4f));

            // 同一个 mesh 重复赋给 sharedMesh 也会发一次 OnMeshFilterChanged，能省就省
            if (_mf != null && _mf.sharedMesh != _mesh) _mf.sharedMesh = _mesh;
        }

        protected void SetGeometry(List<Vector3> v, List<Vector2> uv, List<int> t)
        {
            _mesh.SetVertices(v);
            _mesh.SetUVs(0, uv);
            _mesh.SetTriangles(t, 0);
            _mesh.RecalculateBounds();
        }

        /// <summary>卡牌轮廓（XZ，中心在原点，首尾不重复）。</summary>
        protected List<Vector2> Outline(float outset)
        {
            return CellFXShape.Outline(_card, cornerRadius, cornerSegments, outset);
        }

        /// <summary>一段竖直面。uv.y 0 = 贴地、1 = 顶端（所有 shader 都按这个算高度）。</summary>
        protected static void AddQuad(List<Vector3> v, List<Vector2> uv, List<int> t,
                                      Vector3 a0, Vector3 b0, Vector3 b1, Vector3 a1)
        {
            int i = v.Count;
            v.Add(a0); v.Add(b0); v.Add(b1); v.Add(a1);
            uv.Add(new Vector2(0f, 0f));
            uv.Add(new Vector2(1f, 0f));
            uv.Add(new Vector2(1f, 1f));
            uv.Add(new Vector2(0f, 1f));
            t.Add(i); t.Add(i + 1); t.Add(i + 2);
            t.Add(i); t.Add(i + 2); t.Add(i + 3);
        }

        /// <summary>沿卡牌轮廓立一圈竖直面（墙用）。y0 → y1 是真实高度。</summary>
        protected void BuildRibbon(float outset, float y0, float y1,
                                   List<Vector3> v, List<Vector2> uv, List<int> t)
        {
            var o = Outline(outset);
            for (int i = 0; i < o.Count; i++)
            {
                var a = o[i];
                var b = o[(i + 1) % o.Count];
                if ((a - b).sqrMagnitude < 1e-12f) continue;    // 圆角半径 0 时会重合
                AddQuad(v, uv, t,
                        new Vector3(a.x, y0, a.y), new Vector3(b.x, y0, b.y),
                        new Vector3(b.x, y1, b.y), new Vector3(a.x, y1, a.y));
            }
        }

        /// <summary>
        /// 取这次渲染要用的材质。
        ///
        /// 顺序：字段里指定的 → 编辑器里找同名 .mat 资产（找到就写回字段，Inspector 上才有颜色可调）
        ///      → 运行时临时建一个。
        ///
        /// 临时的那个**不写回字段**：它是 HideAndDontSave 的，写进序列化字段会在存盘时变成空引用。
        /// </summary>
        Material ResolveMaterial()
        {
            if (material != null) return material;

#if UNITY_EDITOR
            var asset = FindMaterialAsset(ShaderName);
            if (asset != null) { material = asset; return asset; }   // 落进字段，此后就能在 Inspector 上改
#endif
            if (_runtimeMat == null) _runtimeMat = FallbackMaterial();
            return _runtimeMat;
        }

        Material FallbackMaterial()
        {
            var sh = Shader.Find(ShaderName);
            if (sh == null)
            {
                Debug.LogWarning("[CellFX] 找不到 shader " + ShaderName +
                                 "，请确认它已导入工程，或用菜单 GameObject/VFX/Board Cell Upgrade 生成。");
                return null;
            }
            return new Material(sh) { name = ShaderName + "_Runtime", hideFlags = HideFlags.HideAndDontSave };
        }

#if UNITY_EDITOR
        /// <summary>按 shader 名找同名 .mat（例如 CellFXFrame → CellFXFrame.mat）。</summary>
        static Material FindMaterialAsset(string shaderName)
        {
            string leaf = shaderName.Substring(shaderName.LastIndexOf('/') + 1);   // CellFXFrame
            var guids = UnityEditor.AssetDatabase.FindAssets(leaf + " t:Material");
            for (int i = 0; i < guids.Length; i++)
            {
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[i]);
                if (System.IO.Path.GetFileNameWithoutExtension(path) != leaf) continue;
                var m = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(path);
                if (m != null && m.shader != null && m.shader.name == shaderName) return m;
            }
            return null;
        }
#endif

        static void ConfigureRenderer(MeshRenderer r)
        {
            if (r == null) return;
            r.shadowCastingMode = ShadowCastingMode.Off;
            r.receiveShadows = false;
            r.lightProbeUsage = LightProbeUsage.Off;
            r.reflectionProbeUsage = ReflectionProbeUsage.Off;
        }

        static T GetOrAdd<T>(GameObject go) where T : Component
        {
            var c = go.GetComponent<T>();
            if (c == null) c = go.AddComponent<T>();
            return c;
        }
    }
}
