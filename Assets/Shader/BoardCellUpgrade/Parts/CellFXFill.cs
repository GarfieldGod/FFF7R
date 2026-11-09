using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace FFF7R.VFX
{
    /// <summary>
    /// 部件 1 · 卡牌实心。
    ///
    /// 形状 = 卡牌那个圆角矩形（0.32 × 0.44，r 0.0174），由片元里的有符号距离裁出来，
    /// 所以比例和圆角都是解析的，跟网格细分无关。
    ///
    /// 它**只管内部那一片亮面**。边界那圈更亮的带是另一个部件（CellFXEdge）——
    /// 拆开之后两层的亮暗节奏可以各调：实心常亮、边框脉冲，或者反过来。
    ///
    /// 高度必须高于卡牌：卡牌顶面 0.020，实心放低了会被整块挡掉、只剩边缘一条缝，
    /// 症状是"实心完全没生效"，那时候调亮度是没用的。
    /// </summary>
    [ExecuteAlways]
    [AddComponentMenu("FFF7R/VFX/Parts/Cell FX Fill")]
    public class CellFXFill : CellFXPart
    {
        [Header("几何")]
        [Tooltip("quad 比卡牌大多少（每边占比）。只为了给边缘软边留几像素，不是视觉尺寸")]
        [Range(0f, 0.2f)] public float pad = 0.02f;

        [Tooltip("实心那层的绝对高度。必须高于卡牌顶面 0.020，否则整块被卡牌挡掉")]
        public float fillLift = 0.028f;

        [Tooltip("卡牌轮廓处的软边宽度（× 卡牌短边）。太小有锯齿，太大整张卡边缘会糊")]
        [Range(0.001f, 0.06f)] public float edgeSoft = 0.012f;

        [Header("存在度")]
        [Tooltip("淡入占的进度。0.10 = 前 10% 用来亮起来")]
        [Range(0.001f, 0.5f)] public float fadeIn = 0.10f;

        [Tooltip("开始熄灭的进度。0.68 × 总时长 = 熄灭时刻")]
        [Range(0.2f, 1f)] public float fadeOut = 0.68f;

        [Header("脉冲（默认关闭）")]
        [Tooltip("0 = 常量亮着；0.4 = 在 60%~100% 之间呼吸。和「淡入淡出」是叠乘的")]
        [Range(0f, 1f)] public float pulseDepth = 0f;

        [Tooltip("整段进度里呼吸几次。3.9 ≈ 全程四个来回")]
        public float pulseSpeed = 3.90f;

        static readonly int QuadWId    = Shader.PropertyToID("_QuadW");
        static readonly int QuadHId    = Shader.PropertyToID("_QuadH");
        static readonly int EdgeSoftId = Shader.PropertyToID("_EdgeSoft");
        static readonly int FadeInId   = Shader.PropertyToID("_FadeIn");
        static readonly int FadeOutId  = Shader.PropertyToID("_FadeOut");
        static readonly int PulseDId   = Shader.PropertyToID("_PulseDepth");
        static readonly int PulseSId   = Shader.PropertyToID("_PulseSpeed");

        float _quadW = 1f, _quadH = 1f;

        protected override string ShaderName { get { return "FFF7/VFX/CellFX/Fill"; } }

        protected override void BuildGeometry(Vector2 card)
        {
            _quadW = card.x * (1f + 2f * pad);
            _quadH = card.y * (1f + 2f * pad);

            float sx = _quadW * 0.5f;
            float sz = _quadH * 0.5f;
            float y  = fillLift;

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
            b.SetFloat(EdgeSoftId, Mathf.Max(edgeSoft, 1e-4f));
            b.SetFloat(FadeInId, Mathf.Max(fadeIn, 1e-4f));
            b.SetFloat(FadeOutId, fadeOut);
            b.SetFloat(PulseDId, pulseDepth);
            b.SetFloat(PulseSId, pulseSpeed);
        }
    }
}
