using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace FFF7R.VFX
{
    /// <summary>
    /// 部件 4 · 平面扩散。一圈**卡牌形状**的框，从卡牌边缘往外推。
    ///
    /// 两个随时间变化的量（都在本组件的 Inspector 上，不用去翻材质）：
    ///   · 线宽    起始宽度 → 末尾宽度（往外推的同时变细，像散开）
    ///   · 透明度  起始透明度 → 末尾透明度（默认 1 → 0，边推边化掉）
    /// 两者都是线性的；想非线性就给部件的 `curve` 换条曲线 ——
    /// 它作用在整个进度上，宽度和透明度会一起跟着变。
    ///
    /// 为什么这些值放在组件上而不是材质上：多份 Spread 可以各调各的。
    /// 材质只留颜色和亮度（同套皮肤共享），形状与端点这类"每个实例不同"的走 MPB。
    ///
    /// `pad` 必须大于「扩散到多远」：否则最后一段被 quad 边界切掉（OnValidate 会 warning）。
    /// </summary>
    [ExecuteAlways]
    [AddComponentMenu("FFF7R/VFX/Parts/Cell FX Spread")]
    public class CellFXSpread : CellFXPart
    {
        [Header("几何")]
        [Tooltip("quad 比卡牌大多少（每边占比）。必须大于「扩散到多远」，否则扩散被切掉")]
        [Range(0.05f, 1.2f)] public float pad = 0.62f;

        [Tooltip("离格子面的高度。低于卡牌顶面 0.020，环在卡牌范围内的那半圈会被卡牌挡住（读作从卡牌底下钻出来）")]
        public float lift = 0.004f;

        [Header("扩散")]
        [Tooltip("推到多远。单位是卡牌短边（0.32 × 这个 = 米）。0.45 ≈ 往外 14 cm")]
        [Range(0.02f, 0.7f)] public float reach = 0.45f;

        [Header("线宽（随进度线性插值）")]
        [Tooltip("刚发出时的线宽，单位是卡牌短边。0.050 ≈ 1.6 cm")]
        [Range(0.002f, 0.15f)] public float widthStart = 0.050f;

        [Tooltip("散尽时的线宽。比起始小就是「一边推一边变细」")]
        [Range(0.002f, 0.15f)] public float widthEnd = 0.014f;

        [Header("透明度（随进度线性插值，纯加色下就是亮度乘数）")]
        [Range(0f, 1.5f)] public float alphaStart = 1.00f;

        [Tooltip("散尽时还剩多少。0 = 完全化掉")]
        [Range(0f, 1.5f)] public float alphaEnd = 0.00f;

        [Tooltip("刚发出那一下的淡入长度（占总进度）。太短会在格边上「啪」地跳出来")]
        [Range(0.001f, 0.3f)] public float fadeIn = 0.05f;

        static readonly int QuadWId     = Shader.PropertyToID("_QuadW");
        static readonly int QuadHId     = Shader.PropertyToID("_QuadH");
        static readonly int ReachId     = Shader.PropertyToID("_Reach");
        static readonly int WidthStartId = Shader.PropertyToID("_WidthStart");
        static readonly int WidthEndId   = Shader.PropertyToID("_WidthEnd");
        static readonly int AlphaStartId = Shader.PropertyToID("_AlphaStart");
        static readonly int AlphaEndId   = Shader.PropertyToID("_AlphaEnd");
        static readonly int FadeInId     = Shader.PropertyToID("_FadeIn");

        float _quadW = 1f, _quadH = 1f;

        protected override string ShaderName { get { return "FFF7/VFX/CellFX/Spread"; } }

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

        /// <summary>参数在编辑器里改过之后，让 Group 重新给一次进度（renderer 可能被关着）。</summary>
        protected override void Push(float p, MaterialPropertyBlock b)
        {
            b.SetFloat(QuadWId, Mathf.Max(_quadW, 1e-4f));
            b.SetFloat(QuadHId, Mathf.Max(_quadH, 1e-4f));
            b.SetFloat(ReachId, reach);
            b.SetFloat(WidthStartId, widthStart);
            b.SetFloat(WidthEndId, widthEnd);
            b.SetFloat(AlphaStartId, alphaStart);
            b.SetFloat(AlphaEndId, alphaEnd);
            b.SetFloat(FadeInId, Mathf.Max(fadeIn, 1e-4f));
        }

        protected override void OnValidate()
        {
            base.OnValidate();

            if (pad < reach)
                Debug.LogWarning("[CellFXSpread] pad (" + pad.ToString("0.00") +
                                 ") 小于「扩散到多远」(" + reach.ToString("0.00") +
                                 ")，扩散环最后一段会被 quad 边界切掉。把 pad 调到 ≥ " +
                                 reach.ToString("0.00") + "。", this);
        }
    }
}
