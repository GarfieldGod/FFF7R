using UnityEngine;
using UnityEngine.Rendering;

namespace FFF7R.VFX
{
    /// <summary>
    /// 卡牌辉芒 · 3D 世界版。挂在棋盘上的**卡牌中心正上方**（Y ≈ 0.0265），
    /// 卡牌边缘会一直向四周发散光芒，光条带扰动、缓慢流动。
    ///
    /// 用法（菜单 GameObject / VFX / Card Aura 一键建）：
    ///   1. 把对象放到卡牌中心，Y 抬到卡牌顶面以上（卡牌顶 0.020 → 放 0.0265）
    ///   2. 外观只在材质（CardAura.mat）上改颜色和亮度
    ///   3. 形状 / 条数 / 扰动 / 速度都在本组件上，不进材质（多个实例可以各不相同）
    ///
    /// **尺寸由组件拥有，但写在网格里，不写在 Transform 上**：
    /// 它按 cardSize / padding 建一张"就是这么大"的 quad 网格，Transform.scale 恒为 (1,1,1)。
    /// 所以 1,1,1 就是正确比例，想改大小只改 cardSize / padding（网格顶点跟着改），
    /// 也可以直接在 Transform 上乘一个系数整体放大 —— 但那样光就不再贴着卡牌了。
    ///
    /// 这是"状态型"特效（可选 / 已激活），挂上就一直发散，不吃进度条；
    /// 开关用 on 勾选框或 SetOn()，内部平滑淡入淡出。
    ///
    /// 编辑器注意：**任何给 sharedMesh 赋值的代码都不能在 OnValidate / OnEnable 里同步执行**，
    /// 详见 ScheduleRefresh 的注释（预制体里 m_Mesh 永远是空的，每次导入都会重赋一次）。
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    [AddComponentMenu("FFF7R/VFX/Card Aura FX")]
    public class CardAuraFX : MonoBehaviour
    {
        [Header("外观")]
        [Tooltip("色调，乘在材质颜色上。多张卡要统一换色就改这里")]
        public Color tint = Color.white;
        [Tooltip("亮度，乘在材质亮度上")]
        [Range(0f, 4f)] public float intensity = 1f;
        [Tooltip("等级 0-3，只影响亮度，不改形状")]
        [Range(0, 3)] public int level = 1;

        [Header("形状（卡牌 8:11 + 圆角）")]
        [Tooltip("卡牌尺寸（米）。0.32 × 0.44 就是卡牌本身的尺寸")]
        public Vector2 cardSize = new Vector2(0.32f, 0.44f);
        [Tooltip("圆角半径（米）。0.0174 就是卡牌几何上的那个圆角")]
        public float cornerRadius = 0.0174f;
        [Tooltip("quad 比卡牌每边大多少（按卡牌短边的比例）。给光芒留出发散的空间，0.30 ≈ 每边 9.6 cm")]
        [Range(0f, 1.5f)] public float padding = 0.30f;

        [Header("光芒")]
        [Tooltip("一圈有多少条光芒（噪声绕一圈跨过的格数）。30 条左右，短边上约 10 条")]
        [Range(6, 160)] public int rayCount = 30;
        [Tooltip("光芒往外发散多远，按卡牌短边的比例。0.20 ≈ 6.4 cm（卡宽 0.32 时）—— 一小层就够了")]
        public float reach = 0.20f;
        [Tooltip("光芒从卡边外多远才开始，按卡牌短边的比例。0 = 贴着卡边起；0.06 ≈ 离边 2 cm 才起光（中间留一圈空）")]
        [Range(0f, 0.5f)] public float offset = 0f;
        [Tooltip("起始强度：光条刚出现那一点的亮度。1 = 起点最亮（贴边一道亮边）；0.60 = 薄雾；0.3 = 从近乎透明慢慢亮起来")]
        [Range(0f, 1f)] public float startAlpha = 0.60f;
        [Tooltip("边缘不规则度。1 = 每条光长短不一、末端毛糙（野生光芒）；" +
                 "0 = 所有光条停在同一条等距线上，光的外沿变成与卡牌边框**完全同形**的圆角矩形")]
        [Range(0f, 1f)] public float edgeIrregular = 1f;
        [Tooltip("扰动强度。0 = 均匀放射（假）；0.45 = 会断开、粗细不匀、缓慢蠕动")]
        [Range(0f, 1.5f)] public float turbulence = 0.45f;
        [Tooltip("弯曲量（弧度 / 卡牌短边）。一次项 ∝ 离卡边的距离：整根光往一侧甩成曲线。0 = 直射线")]
        [Range(0f, 6f)] public float bend = 1.6f;
        [Tooltip("S 形弯曲（二次项 ∝ 距离²）。远处把光翻回来，成蛇形。0 = 只单侧弯（C 形）")]
        [Range(0f, 6f)] public float bendS = 1f;
        [Tooltip("流动速度。0 = 冻住；1 = 每秒漂过约 1/3 个噪声格")]
        [Range(0f, 4f)] public float flow = 1f;
        [Tooltip("整圈光芒缓慢旋转（度/秒）。0 = 不转")]
        [Range(-40f, 40f)] public float spin = 5f;
        [Tooltip("径向衰减指数。越大越贴边，光收得越快")]
        [Range(0.5f, 5f)] public float falloff = 1.05f;
        [Tooltip("卡牌轮廓上那圈细亮线的宽度（按卡牌短边的比例）。0.022 ≈ 7 mm")]
        [Range(0.004f, 0.12f)] public float rimWidth = 0.022f;
        [Tooltip("卡牌轮廓上那圈细亮线的亮度。0 = 不要这圈线")]
        [Range(0f, 3f)] public float rimGain = 0.55f;
        [Tooltip("贴边柔光（对称在卡边内外），把卡牌从棋盘上托起来")]
        [Range(0f, 3f)] public float haloGain = 0.32f;

        [Header("开关")]
        [Tooltip("勾上 = 正在发散。取消 = 淡出到完全不可见（渲染器也会关掉）")]
        public bool on = true;
        [Tooltip("淡入淡出速度（每秒走多少 0..1）。6 ≈ 0.17 s 亮起来")]
        [Range(0.5f, 40f)] public float fadeSpeed = 6f;

        [Header("材质")]
        [Tooltip("留空 = 自动找同目录 Materials/CardAura.mat，找不到就建一个运行时临时材质")]
        public Material material;

        // 旧版把尺寸写在 localScale 上（(w, 1, h)）。现在尺寸烘进网格，scale 必须是 1 ——
        // 场景里已有的实例还带着旧 scale，不清掉会双重重缩。这个标记只让它跑一次。
        [SerializeField, HideInInspector] bool _layoutBaked;

        static readonly int TintId       = Shader.PropertyToID("_Tint");
        static readonly int IntensityId  = Shader.PropertyToID("_Intensity");
        static readonly int LevelId      = Shader.PropertyToID("_Level");
        static readonly int OnId         = Shader.PropertyToID("_On");
        static readonly int RayCountId   = Shader.PropertyToID("_RayCount");
        static readonly int ReachId      = Shader.PropertyToID("_Reach");
        static readonly int OffsetId     = Shader.PropertyToID("_Offset");
        static readonly int StartAlphaId = Shader.PropertyToID("_StartAlpha");
        static readonly int EdgeIrrId    = Shader.PropertyToID("_EdgeIrr");
        static readonly int TurbulenceId = Shader.PropertyToID("_Turbulence");
        static readonly int BendId       = Shader.PropertyToID("_Bend");
        static readonly int BendSId      = Shader.PropertyToID("_BendS");
        static readonly int FlowId       = Shader.PropertyToID("_Flow");
        static readonly int SpinId       = Shader.PropertyToID("_Spin");
        static readonly int FalloffId    = Shader.PropertyToID("_Falloff");
        static readonly int RimWidthId   = Shader.PropertyToID("_RimWidth");
        static readonly int RimGainId    = Shader.PropertyToID("_RimGain");
        static readonly int HaloGainId   = Shader.PropertyToID("_HaloGain");
        static readonly int HalfWId      = Shader.PropertyToID("_HalfW");
        static readonly int HalfDId      = Shader.PropertyToID("_HalfD");
        static readonly int RadiusId     = Shader.PropertyToID("_Radius");
        static readonly int QuadWId      = Shader.PropertyToID("_QuadW");
        static readonly int QuadHId      = Shader.PropertyToID("_QuadH");
        static readonly int UnitId       = Shader.PropertyToID("_Unit");

        const string QuadName = "CardAura_Quad";
        const string ShaderName = "FFF7/VFX/CardAura";

        // 每个组件一张自己的 quad，尺寸烘在顶点里 —— 不共享，省得拖 padding 时按尺寸
        // 堆出一大堆网格；也不在 MeshFilter 换引用，所以改尺寸不会发 OnMeshFilterChanged。
        Mesh _quad;

        MeshFilter _mf;
        MeshRenderer _mr;
        MaterialPropertyBlock _mpb;
        Material _runtimeMat;              // 只在找不到 .mat 资产时才有（不落盘）

        float _on;                         // 平滑后的开关值（写进 shader）
        float _lastPushedOn = -1f;
        float _quadW = -1f, _quadH = -1f;  // 想要的目标尺寸（米）
        bool _dirty = true;

        // ---- 生命周期 -------------------------------------------------

        protected virtual void OnEnable()
        {
            _on = on ? 1f : 0f;            // 挂上就亮，不要让新对象从全黑淡进来
            _dirty = true;

#if UNITY_EDITOR
            // 编辑器里 OnEnable 也可能是场景加载 / 预制体导入顺带触发的，
            // 那一刻同样还在 CheckConsistency 阶段，碰 sharedMesh 会报错。统一走延迟。
            if (!Application.isPlaying)
            {
                ScheduleRefresh();
                return;
            }
#endif
            FitQuad();                     // 先把尺寸算出来，网格才知道该建多大
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
            _dirty = true;
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
            if (UnityEditor.PrefabUtility.IsPartOfPrefabAsset(gameObject))
            {
                FitQuad();
                Apply(true);
                return;
            }

            FitQuad();
            EnsureMesh();
            Apply(true);
        }
#endif

        protected virtual void Update()
        {
            // 兜底：域重载会清空字段，MeshFilter 上的网格也可能被销毁。
            // Update 一定不在禁发消息的阶段里，在这里补建最安全；只补 null / 尺寸不对的那张，
            // 不抢别人塞的网格。注意顺序：先 FitQuad 把目标尺寸算出来。
            FitQuad();
            if (NeedsMesh()) EnsureMesh();

            StepEnvelope();
            Apply(false);
        }

        protected virtual void OnDisable()
        {
            // 组件关掉后不会再 Apply，renderer 还挂着、材质冻在最后一帧 —— 一并关掉
            if (_mr != null) _mr.enabled = false;
        }

        protected virtual void OnDestroy()
        {
            if (_runtimeMat != null)                    // 临时材质要自己收，否则每次重建都漏一个
            {
                if (Application.isPlaying) Destroy(_runtimeMat);
                else DestroyImmediate(_runtimeMat);
                _runtimeMat = null;
            }

            // 网格是 HideAndDontSave 的，不随物体销毁，得自己收
            if (_quad != null)
            {
                if (Application.isPlaying) Destroy(_quad);
                else DestroyImmediate(_quad);
                _quad = null;
            }
        }

        // ---- 对外接口 ------------------------------------------------

        /// <summary>开 / 关（平滑淡入淡出，不会硬切）。</summary>
        public void SetOn(bool value)
        {
            on = value;
            // 组件关着的时候没人调 Update，过渡跑不起来 —— 直接到位
            if (!isActiveAndEnabled) SnapOn(value);
        }

        /// <summary>立即到位，不做过渡（切场景、跳过动画时用）。</summary>
        public void SnapOn(bool value)
        {
            on = value;
            _on = value ? 1f : 0f;
            _dirty = true;
            FitQuad();
            Apply(true);
        }

        /// <summary>整套配色换色。0-3 级只影响亮度。</summary>
        public void SetTint(Color c, int lv = -1)
        {
            tint = c;
            if (lv >= 0) level = Mathf.Clamp(lv, 0, 3);
            _dirty = true;
        }

        /// <summary>改完 cardSize / padding 之后需要立刻重算时调一次（Update 里也会追）。</summary>
        public void Refit()
        {
            FitQuad();
            EnsureMesh();          // 尺寸变了要就地改顶点（没建过就建一张）
            Apply(true);
        }

        // ---- 内部 ----------------------------------------------------

        void StepEnvelope()
        {
            float target = on ? 1f : 0f;
            if (Mathf.Approximately(_on, target)) return;
            _on = Mathf.MoveTowards(_on, target, Mathf.Max(fadeSpeed, 1e-3f) * Time.deltaTime);
            _dirty = true;
        }

        /// <summary>目标 quad 尺寸 = 卡牌尺寸 × (1 + 2·padding)。只记数字，网格在 EnsureMesh 里换。</summary>
        void FitQuad()
        {
            BakeLayoutOnce();

            float k = 1f + 2f * Mathf.Max(padding, 0f);
            float w = Mathf.Max(cardSize.x, 1e-3f) * k;
            float h = Mathf.Max(cardSize.y, 1e-3f) * k;

            if (Mathf.Abs(w - _quadW) < 1e-5f && Mathf.Abs(h - _quadH) < 1e-5f) return;
            _quadW = w;
            _quadH = h;
            _dirty = true;
        }

        /// <summary>
        /// 把尺寸从 Transform 迁到网格上，只做一次。
        /// 旧版的 quad 是 1×1，尺寸写在 localScale 上（(0.544, 1, 0.748) 那种），
        /// 现在网格自己就是目标尺寸、scale 必须是 1 —— 场景里已有的实例还带着旧 scale，
        /// 不归零就会双重重缩（网格 × scale）。
        /// </summary>
        void BakeLayoutOnce()
        {
            if (_layoutBaked) return;
            _layoutBaked = true;
            if (transform.localScale != Vector3.one) transform.localScale = Vector3.one;
        }

        /// <summary>把参数写进渲染器。force = 忽略"没变化"的短路。</summary>
        public void Apply(bool force)
        {
            if (_mr == null) _mr = GetComponent<MeshRenderer>();
            if (_mr == null) return;

            if (!force && !_dirty && Mathf.Approximately(_on, _lastPushedOn)) return;
            _dirty = false;
            _lastPushedOn = _on;

            var mat = ResolveMaterial();
            if (mat != null && _mr.sharedMaterial != mat) _mr.sharedMaterial = mat;

            if (_mpb == null) _mpb = new MaterialPropertyBlock();
            _mr.GetPropertyBlock(_mpb);

            float unit = Mathf.Max(Mathf.Min(cardSize.x, cardSize.y), 1e-4f);
            _mpb.SetColor(TintId, tint);
            _mpb.SetFloat(IntensityId, intensity);
            _mpb.SetFloat(LevelId, level);
            _mpb.SetFloat(OnId, _on);
            _mpb.SetFloat(RayCountId, Mathf.Max(rayCount, 4));
            _mpb.SetFloat(ReachId, Mathf.Max(reach, 0f));
            _mpb.SetFloat(OffsetId, Mathf.Clamp(offset, 0f, 0.5f));
            _mpb.SetFloat(StartAlphaId, Mathf.Clamp01(startAlpha));
            _mpb.SetFloat(EdgeIrrId, Mathf.Clamp01(edgeIrregular));
            _mpb.SetFloat(TurbulenceId, Mathf.Max(turbulence, 0f));
            _mpb.SetFloat(BendId, Mathf.Max(bend, 0f));
            _mpb.SetFloat(BendSId, Mathf.Max(bendS, 0f));
            _mpb.SetFloat(FlowId, Mathf.Max(flow, 0f));
            _mpb.SetFloat(SpinId, spin);
            _mpb.SetFloat(FalloffId, Mathf.Max(falloff, 0.1f));
            _mpb.SetFloat(RimWidthId, Mathf.Max(rimWidth, 0f));
            _mpb.SetFloat(RimGainId, Mathf.Max(rimGain, 0f));
            _mpb.SetFloat(HaloGainId, Mathf.Max(haloGain, 0f));
            _mpb.SetFloat(HalfWId, Mathf.Max(cardSize.x, 1e-3f) * 0.5f);
            _mpb.SetFloat(HalfDId, Mathf.Max(cardSize.y, 1e-3f) * 0.5f);
            _mpb.SetFloat(RadiusId, Mathf.Max(cornerRadius, 1e-5f));
            _mpb.SetFloat(QuadWId, Mathf.Max(_quadW, 1e-3f));
            _mpb.SetFloat(QuadHId, Mathf.Max(_quadH, 1e-3f));
            _mpb.SetFloat(UnitId, unit);

            _mr.SetPropertyBlock(_mpb);
            _mr.enabled = _on > 1e-3f;      // 全黑就彻底不画，不留在场景里
        }

        bool NeedsMesh()
        {
            if (_mf == null) _mf = GetComponent<MeshFilter>();
            if (_mf == null) return false;
            var m = _mf.sharedMesh;
            if (!IsOurMesh(m)) return true;
            return !SizeMatches(m, _quadW, _quadH);
        }

        void EnsureMesh()
        {
            if (_mf == null) _mf = GetComponent<MeshFilter>();
            if (_mr == null) _mr = GetComponent<MeshRenderer>();
            if (_mf == null) return;

            ConfigureRenderer(_mr);

#if UNITY_EDITOR
            // 预制体资产上不建网格：那个网格是 HideAndDontSave，存不进 .prefab
            // （存下来也只有 m_Mesh: fileID 0），每次导入都白建一份，还会把资产弄脏。
            if (!Application.isPlaying && UnityEditor.PrefabUtility.IsPartOfPrefabAsset(gameObject)) return;
#endif

            var existing = _mf.sharedMesh;
            if (IsOurMesh(existing))
            {
                // 域重载会把字段清空，但网格对象本身还挂在 MeshFilter 上 —— 认回来，别再赋一次
                // （赋值 = 又一次 OnMeshFilterChanged）。尺寸不对时**就地改顶点**，也不换引用。
                _quad = existing;
                ReshapeQuad(_quad, _quadW, _quadH);
                return;
            }

            _quad = BuildQuad(_quadW, _quadH);
            _mf.sharedMesh = _quad;        // 只有这一行会发 OnMeshFilterChanged
        }

        static bool IsOurMesh(Mesh m)
        {
            return m != null && m.name.StartsWith(QuadName, System.StringComparison.Ordinal);
        }

        /// <summary>网格自带的尺寸对不对得上。quad 躺在 XZ 平面，所以看 bounds 的 x / z。</summary>
        static bool SizeMatches(Mesh m, float w, float h)
        {
            var s = m.bounds.size;
            return Mathf.Abs(s.x - w) < 1e-4f && Mathf.Abs(s.z - h) < 1e-4f;
        }

        static Vector3[] QuadVerts(float w, float h)
        {
            float hw = w * 0.5f, hh = h * 0.5f;
            return new[]
            {
                new Vector3(-hw, 0f, -hh),
                new Vector3( hw, 0f, -hh),
                new Vector3( hw, 0f,  hh),
                new Vector3(-hw, 0f,  hh),
            };
        }

        /// <summary>改尺寸只改顶点（渲染器重新读几何，不发消息，也不会把预制体弄脏）。</summary>
        static void ReshapeQuad(Mesh m, float w, float h)
        {
            if (SizeMatches(m, w, h)) return;
            m.vertices = QuadVerts(w, h);
            m.RecalculateBounds();
        }

        static Mesh BuildQuad(float w, float h)
        {
            // 网格**本身就是目标尺寸**（不是 1×1 再靠 scale 拉），所以 Transform 可以是 1,1,1。
            var m = new Mesh
            {
                name = QuadName + "_" + w.ToString("F3") + "x" + h.ToString("F3"),
                hideFlags = HideFlags.HideAndDontSave,
            };

            m.vertices = QuadVerts(w, h);          // 中心在原点，躺在 XZ 平面，法线 +Y
            m.uv = new[]
            {
                new Vector2(0f, 0f), new Vector2(1f, 0f),
                new Vector2(1f, 1f), new Vector2(0f, 1f),
            };
            m.colors = new[] { Color.white, Color.white, Color.white, Color.white };
            m.normals = new[] { Vector3.up, Vector3.up, Vector3.up, Vector3.up };
            m.triangles = new[] { 0, 2, 1, 0, 3, 2 };   // 这个绕序才是 +Y 朝上
            m.RecalculateBounds();

            return m;
        }

        static void ConfigureRenderer(MeshRenderer r)
        {
            if (r == null) return;
            r.shadowCastingMode = ShadowCastingMode.Off;
            r.receiveShadows = false;
            r.lightProbeUsage = LightProbeUsage.Off;
            r.reflectionProbeUsage = ReflectionProbeUsage.Off;
        }

        // ---- 材质 ----------------------------------------------------

        Material ResolveMaterial()
        {
            if (material != null) return material;

#if UNITY_EDITOR
            var asset = FindMaterialAsset();
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
                Debug.LogWarning("[CardAura] 找不到 shader " + ShaderName +
                                 "，请确认它已导入工程，或用菜单 GameObject/VFX/Card Aura 生成。");
                return null;
            }
            return new Material(sh) { name = ShaderName + "_Runtime", hideFlags = HideFlags.HideAndDontSave };
        }

#if UNITY_EDITOR
        static Material FindMaterialAsset()
        {
            var guids = UnityEditor.AssetDatabase.FindAssets("CardAura t:Material");
            for (int i = 0; i < guids.Length; i++)
            {
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[i]);
                if (System.IO.Path.GetFileNameWithoutExtension(path) != "CardAura") continue;
                var m = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(path);
                if (m != null && m.shader != null && m.shader.name == ShaderName) return m;
            }
            return null;
        }
#endif
    }
}
