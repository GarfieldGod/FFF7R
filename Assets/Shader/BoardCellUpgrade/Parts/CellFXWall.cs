using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace FFF7R.VFX
{
    /// <summary>
    /// 部件 3 · 光墙。顺着**卡牌轮廓**（含四角圆弧）立起来的一圈竖直面。
    ///
    /// 行为：从下往上涨 → 停住 → **消散**（不再是"落下去"）。
    /// 消散有两种口味，由「消散方式」在两者之间插值：
    ///   · 0 = 整体淡出
    ///   · 1 = 从下往上抽走（底边先化，顶边最后没）
    ///
    /// 涨多快、什么时候开始散、散得多软都在本组件的 Inspector 上；
    /// 走多久看这个组件的 `duration`（或 Group 时间表槽上的 duration）。
    /// 想让墙只升不散，把驱动方给的进度停在「开始消散的进度」之前即可。
    /// </summary>
    [ExecuteAlways]
    [AddComponentMenu("FFF7R/VFX/Parts/Cell FX Wall")]
    public class CellFXWall : CellFXPart
    {
        [Header("几何")]
        [Tooltip("墙高 = 卡牌深 × 这个")]
        public float heightScale = 0.90f;

        [Tooltip("轮廓整体外扩（米）。0 = 正好贴在卡牌边上；墙在竖直方向上不和卡牌抢深度，一般不用动")]
        public float outset = 0f;

        [Tooltip("墙根离格子面的高度，防 z-fighting")]
        public float lift = 0.002f;

        [Header("涨与散")]
        [Tooltip("涨满时的进度。0.18 = 全程的前 18% 用来升起，之后是「墙立着」")]
        [Range(0.02f, 0.9f)] public float riseEnd = 0.18f;

        [Tooltip("升满时顶端高出多少（1.18 = 多出 18%）。留这点余量，顶端才是「漫上来」而不是被切一刀")]
        [Range(0.8f, 1.4f)] public float headScale = 1.18f;

        [Tooltip("开始消散的进度。要在两个框散尽之前一点点，读起来才是「框快没了，底下的光也跟着收」")]
        [Range(0.1f, 0.95f)] public float dissolveStart = 0.55f;

        [Tooltip("消散前沿的软边厚度。越大越像雾化，越小越像被一条线刮掉")]
        [Range(0.02f, 0.6f)] public float dissolveSoft = 0.28f;

        [Tooltip("0 = 整体淡出（整面一起变淡）；1 = 从下往上抽走（底边先化，顶边最后没）")]
        [Range(0f, 1f)] public float dissolveSweep = 1.0f;

        protected override string ShaderName { get { return "FFF7/VFX/CellFX/Wall"; } }

        static readonly int RiseEndId       = Shader.PropertyToID("_RiseEnd");
        static readonly int HeadScaleId     = Shader.PropertyToID("_HeadScale");
        static readonly int DissolveStartId = Shader.PropertyToID("_DissolveStart");
        static readonly int DissolveSoftId  = Shader.PropertyToID("_DissolveSoft");
        static readonly int DissolveSweepId = Shader.PropertyToID("_DissolveSweep");

        protected override void BuildGeometry(Vector2 card)
        {
            var v = new List<Vector3>(64);
            var uv = new List<Vector2>(64);
            var t = new List<int>(96);

            BuildRibbon(outset, lift, lift + card.y * heightScale, v, uv, t);
            SetGeometry(v, uv, t);
        }

        protected override void Push(float p, MaterialPropertyBlock b)
        {
            b.SetFloat(RiseEndId, Mathf.Max(riseEnd, 1e-3f));
            b.SetFloat(HeadScaleId, headScale);
            b.SetFloat(DissolveStartId, dissolveStart);
            b.SetFloat(DissolveSoftId, Mathf.Max(dissolveSoft, 1e-3f));
            b.SetFloat(DissolveSweepId, dissolveSweep);
        }
    }
}
