using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace FFF7R.VFX
{
    /// <summary>
    /// 部件 2 · 卡牌边框（那一条更亮的边）。
    ///
    /// 原来是和实心挤在同一个 shader 里的，拆出来之后两层的亮暗节奏互不影响：
    /// 实心可以常亮，边框可以脉冲；或者实心先灭、边框还留一下。
    ///
    /// 位置：d = 0 正好是卡牌轮廓，也就是光墙的墙根。
    /// 这三样东西（Fill 的边界 / Edge / Wall 的墙根）在空间上完全重合，
    /// 所以配色要一起定，否则会看到几圈颜色不同的亮边叠在一起。
    ///
    /// 亮带是**跨边界**的：一半压在卡牌边上、一半落在格子面上，
    /// 所以它读起来是"卡牌在发光"，而不是"格子上画了个框"。
    /// </summary>
    [ExecuteAlways]
    [AddComponentMenu("FFF7R/VFX/Parts/Cell FX Edge")]
    public class CellFXEdge : CellFXPart
    {
        [Header("几何")]
        [Tooltip("quad 比卡牌大多少（每边占比）。要给亮带的外半圈留空间，别小于 0.05")]
        [Range(0f, 0.2f)] public float pad = 0.08f;

        [Tooltip("高度。视觉上它和 Fill 是同一层，稍高一点避免和 Fill 抢深度")]
        public float lift = 0.029f;

        [Tooltip("边框宽度 = 卡牌短边 × 这个。0.050 ≈ 1.6 cm，是「亮带整条看起来多宽」")]
        [Range(0.004f, 0.2f)] public float edgeWidth = 0.050f;

        [Header("存在度")]
        [Tooltip("淡入占的进度")]
        [Range(0.001f, 0.5f)] public float fadeIn = 0.08f;

        [Tooltip("开始熄灭的进度。常比实心晚一点，就是「实心先暗下去、边还留一下」")]
        [Range(0.2f, 1f)] public float fadeOut = 0.70f;

        [Header("脉冲（默认关闭）")]
        [Tooltip("0 = 常量；给 0.4 就变成呼吸。和实心的脉冲各走各的")]
        [Range(0f, 1f)] public float pulseDepth = 0f;

        [Tooltip("整段进度里呼吸几次")]
        public float pulseSpeed = 2.60f;

        static readonly int QuadWId    = Shader.PropertyToID("_QuadW");
        static readonly int QuadHId    = Shader.PropertyToID("_QuadH");
        static readonly int WidthId    = Shader.PropertyToID("_EdgeWidth");
        static readonly int FadeInId   = Shader.PropertyToID("_FadeIn");
        static readonly int FadeOutId  = Shader.PropertyToID("_FadeOut");
        static readonly int PulseDId   = Shader.PropertyToID("_PulseDepth");
        static readonly int PulseSId   = Shader.PropertyToID("_PulseSpeed");

        float _quadW = 1f, _quadH = 1f;

        protected override string ShaderName { get { return "FFF7/VFX/CellFX/Edge"; } }

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
            b.SetFloat(WidthId, Mathf.Max(edgeWidth, 1e-4f));
            b.SetFloat(FadeInId, Mathf.Max(fadeIn, 1e-4f));
            b.SetFloat(FadeOutId, fadeOut);
            b.SetFloat(PulseDId, pulseDepth);
            b.SetFloat(PulseSId, pulseSpeed);
        }
    }
}
