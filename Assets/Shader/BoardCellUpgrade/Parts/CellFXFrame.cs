using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace FFF7R.VFX
{
    /// <summary>
    /// 部件 5 · 垂直边框。**一条**框，贴着卡牌轮廓（含圆弧）往上飞出去。
    ///
    /// 一条框一个组件：要两条就在组里挂两个 Frame，各带自己的开始时刻
    /// （第二个用「跟随某部件 70%」触发），也各带**自己的**起始/末尾宽度与透明度。
    /// 所以"第一条粗一点、第二条细一点"是配置，不是改材质 —— 它们共用一个 .mat，
    /// 形状与端点这类"每个实例不同"的量都走组件的 MPB，不进材质。
    ///
    /// 位移是真的动顶点（vert 里 pos.y = 底 + 进度 × 高度 + uv.y × 当前线宽），所以它是
    /// "飞出去"而不是"在墙上画了一道横纹"；线宽也在 vert 里算，于是"起飞时粗、散尽时细"
    /// 是几何变化，不需要额外网格。
    ///
    /// outset 必须大于 0：和墙面共面时，它看着就是墙上的一道横向亮纹，而不是一条独立的框。
    /// </summary>
    [ExecuteAlways]
    [AddComponentMenu("FFF7R/VFX/Parts/Cell FX Frame")]
    public class CellFXFrame : CellFXPart
    {
        [Header("几何")]
        [Tooltip("离墙面那条缝 = 卡牌短边 × 这个。0 时框和墙共面，会看成墙上的横纹")]
        public float outset = 0.014f;

        [Tooltip("墙根离格子面的高度")]
        public float lift = 0.002f;

        [Header("飞行")]
        [Tooltip("往上飞多高。单位是卡牌短边（0.32 × 1.30 ≈ 0.42 m）。顶点真的搬这么远")]
        [Range(0f, 3f)] public float rise = 1.30f;

        [Header("线宽（随进度线性插值）")]
        [Tooltip("起飞时的线宽，单位是卡牌短边。0.090 ≈ 2.9 cm")]
        [Range(0.005f, 0.4f)] public float widthStart = 0.090f;

        [Tooltip("散尽时的线宽。比起始小就是「越飞越细」，这是真实的几何变细")]
        [Range(0.005f, 0.4f)] public float widthEnd = 0.040f;

        [Tooltip("线宽内部的高斯柔和度。越小越像一条硬线，越大越像一团光")]
        [Range(0.08f, 1f)] public float soft = 0.32f;

        [Header("透明度（随进度线性插值，纯加色下就是亮度乘数）")]
        [Range(0f, 1.5f)] public float alphaStart = 1.00f;

        [Tooltip("飞到头时还剩多少。0 = 完全化掉")]
        [Range(0f, 1.5f)] public float alphaEnd = 0.00f;

        [Tooltip("起飞那一下的淡入长度（占总进度）。太短会「啪」地凭空出现")]
        [Range(0.001f, 0.3f)] public float fadeIn = 0.08f;

        protected override string ShaderName { get { return "FFF7/VFX/CellFX/Frame"; } }

        static readonly int LiftId       = Shader.PropertyToID("_BaseLift");
        static readonly int RiseId       = Shader.PropertyToID("_Rise");
        static readonly int WidthStartId = Shader.PropertyToID("_WidthStart");
        static readonly int WidthEndId   = Shader.PropertyToID("_WidthEnd");
        static readonly int SoftId       = Shader.PropertyToID("_Soft");
        static readonly int AlphaStartId = Shader.PropertyToID("_AlphaStart");
        static readonly int AlphaEndId   = Shader.PropertyToID("_AlphaEnd");
        static readonly int FadeInId     = Shader.PropertyToID("_FadeIn");

        protected override void BuildGeometry(Vector2 card)
        {
            var v = new List<Vector3>(64);
            var uv = new List<Vector2>(64);
            var t = new List<int>(96);

            // y 只给 0..1：真正的上下沿在 vert 里按「当前线宽」算，这样线宽才能随时间变
            BuildRibbon(outset, 0f, 1f, v, uv, t);
            SetGeometry(v, uv, t);
        }

        protected override void Push(float p, MaterialPropertyBlock b)
        {
            // 位移与线宽在 vert 里算，片元只吃进度和透明度
            b.SetFloat(LiftId, lift);
            b.SetFloat(RiseId, rise);
            b.SetFloat(WidthStartId, widthStart);
            b.SetFloat(WidthEndId, widthEnd);
            b.SetFloat(SoftId, Mathf.Max(soft, 1e-3f));
            b.SetFloat(AlphaStartId, alphaStart);
            b.SetFloat(AlphaEndId, alphaEnd);
            b.SetFloat(FadeInId, Mathf.Max(fadeIn, 1e-4f));
        }
    }
}
