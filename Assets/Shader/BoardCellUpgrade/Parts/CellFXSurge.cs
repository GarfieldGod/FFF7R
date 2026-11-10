using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace FFF7R.VFX
{
    /// <summary>
    /// 部件 6 · 内涌。波前从**卡牌中心**扩散到**卡牌边缘**，推到边就停，不出卡。
    ///
    /// 和另外两个平面部件的分工：
    ///   Fill    整张卡一起亮，没有起点
    ///   Spread  从卡边往**外**推，越推越淡
    ///   Surge   从卡心往**边缘**推，推到边缘为止（就是这个）
    ///
    /// 波前是一张"小号卡牌"：进度 s 从 0 长到 1，s = 1 时正好压在卡牌轮廓上，
    /// 四条边同时到（不是先到短边再到长边那种歪的波 —— 卡是 8:11，按到边距离做会很明显）。
    ///
    /// 两个形态参数：
    ///   `frontWidth`  波前那道环多粗
    ///   `tailGain`    扫过之后留不留余辉。0 = 只有一圈环在推；1 = 扫过的地方全亮
    ///
    /// 高度必须高于卡牌顶面 0.020（和 Fill 一样），放低了整块被卡牌挡掉。
    /// </summary>
    [ExecuteAlways]
    [AddComponentMenu("FFF7R/VFX/Parts/Cell FX Surge")]
    public class CellFXSurge : CellFXPart
    {
        [Header("几何")]
        [Tooltip("quad 比卡牌大多少（每边占比）。内涌不出卡，这个只留给边缘软边用")]
        [Range(0f, 0.2f)] public float pad = 0.02f;

        [Tooltip("高度。必须高于卡牌顶面 0.020，否则整块被卡牌挡掉（和 Fill 同一个坑）")]
        public float lift = 0.0265f;

        [Tooltip("卡牌轮廓处的软边宽度（× 卡牌短边）。太小有锯齿，太大整张卡边缘会糊")]
        [Range(0.001f, 0.06f)] public float edgeSoft = 0.010f;

        [Header("扩散")]
        [Tooltip("波前冲到卡边占的进度。0.70 = 前 70% 从中心推到边缘，剩下 30% 停在边上熄灭")]
        [Range(0.2f, 1f)] public float sweep = 0.70f;

        [Tooltip("波前那道环的宽度，单位是卡牌短边。0.055 ≈ 1.8 cm")]
        [Range(0.01f, 0.30f)] public float frontWidth = 0.055f;

        [Tooltip("扫过之后留下的余辉。0 = 只有一圈环在推；1 = 扫过的地方全亮（最后整张卡被填满）")]
        [Range(0f, 1.5f)] public float tailGain = 0.30f;

        [Header("存在度")]
        [Tooltip("淡入占的进度。0.05 = 前 5% 用来亮起来，太短会在卡心「啪」地跳出来")]
        [Range(0.001f, 0.5f)] public float fadeIn = 0.05f;

        [Tooltip("开始熄灭的进度。0.86 × 总时长 = 熄灭时刻。要比 sweep 晚，否则还没推到边就暗了")]
        [Range(0.2f, 1f)] public float fadeOut = 0.86f;

        static readonly int QuadWId    = Shader.PropertyToID("_QuadW");
        static readonly int QuadHId    = Shader.PropertyToID("_QuadH");
        static readonly int SweepId    = Shader.PropertyToID("_Sweep");
        static readonly int FrontWId   = Shader.PropertyToID("_FrontWidth");
        static readonly int TailId     = Shader.PropertyToID("_TailGain");
        static readonly int EdgeSoftId = Shader.PropertyToID("_EdgeSoft");
        static readonly int FadeInId   = Shader.PropertyToID("_FadeIn");
        static readonly int FadeOutId  = Shader.PropertyToID("_FadeOut");

        float _quadW = 1f, _quadH = 1f;

        protected override string ShaderName { get { return "FFF7/VFX/CellFX/Surge"; } }

        protected override void BuildGeometry(Vector2 card)
        {
            _quadW = card.x * (1f + 2f * pad);
            _quadH = card.y * (1f + 2f * pad);

            float sx = _quadW * 0.5f;
            float sz = _quadH * 0.5f;
            float y  = lift;

            var v = new List<Vector3>
            {
                new Vector3(-sx, y, -sz), new Vector3(sx, y, -sz),
                new Vector3(sx, y, sz),   new Vector3(-sx, y, sz),
            };
            var uv = new List<Vector2>
            {
                new Vector2(0f, 0f), new Vector2(1f, 0f),
                new Vector2(1f, 1f), new Vector2(0f, 1f),
            };
            var t = new List<int> { 0, 2, 1, 0, 3, 2 };

            SetGeometry(v, uv, t);
        }

        protected override void Push(float p, MaterialPropertyBlock b)
        {
            b.SetFloat(QuadWId, Mathf.Max(_quadW, 1e-4f));
            b.SetFloat(QuadHId, Mathf.Max(_quadH, 1e-4f));
            b.SetFloat(SweepId, Mathf.Clamp(sweep, 1e-3f, 1f));
            b.SetFloat(FrontWId, Mathf.Max(frontWidth, 1e-3f));
            b.SetFloat(TailId, tailGain);
            b.SetFloat(EdgeSoftId, Mathf.Max(edgeSoft, 1e-4f));
            b.SetFloat(FadeInId, Mathf.Max(fadeIn, 1e-4f));
            b.SetFloat(FadeOutId, fadeOut);
        }

        protected override void OnValidate()
        {
            base.OnValidate();

            if (lift < 0.0205f)
                Debug.LogWarning("[CellFXSurge] lift (" + lift.ToString("0.0000") +
                                 ") 不高于卡牌顶面 0.020，内涌会被卡牌整块挡住、只剩边缘一条缝。" +
                                 "调到 0.0265 左右。", this);

            if (fadeOut < sweep)
                Debug.LogWarning("[CellFXSurge] fadeOut (" + fadeOut.ToString("0.00") +
                                 ") 早于 sweep (" + sweep.ToString("0.00") +
                                 ")，波前还没推到卡边就开始暗了。fadeOut 要 ≥ sweep。", this);
        }
    }
}
