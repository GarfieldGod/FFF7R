using System.Collections.Generic;
using UnityEngine;

namespace FFF7R.VFX
{
    /// <summary>
    /// 卡牌轮廓。所有部件共用这一套：圆角矩形，中心在原点，躺在 XZ 平面上。
    ///
    /// 为什么不再按格子：格子是 0.3636 × 0.5 的矩形，卡牌是 0.32 × 0.44 的圆角矩形（8:11）。
    /// 特效如果按格子做，边框就是"格子边"，和卡牌之间还差一圈留白，看着永远是两块东西。
    /// 按卡牌做，边框正好压在卡牌边上，才是一体的。
    ///
    /// 圆角用**圆弧细分**（seg 段），不是倒角：墙和垂直边框要顺着这个轮廓立起来，
    /// 少了圆弧就会在四角看到四个直角缺口。
    /// </summary>
    public static class CellFXShape
    {
        public const float CardW   = 0.32f;      // 卡牌宽（米）
        public const float CardD   = 0.44f;      // 卡牌深（米），8:11
        public const float CornerR = 0.0174f;    // 卡牌几何本身的圆角

        /// <summary>
        /// 闭合轮廓点（XZ）。首尾**不重复**，用时按 (i, i+1) 环绕取。
        /// 每条直边由两个相邻圆弧端点直接连成，所以返回点里没有单独的直边采样。
        /// </summary>
        /// <param name="size">整体尺寸（宽, 深）</param>
        /// <param name="radius">圆角半径（不含 outset）</param>
        /// <param name="seg">每个圆角分几段</param>
        /// <param name="outset">整体向外偏移多少（负值 = 向内）</param>
        public static List<Vector2> Outline(Vector2 size, float radius, int seg, float outset)
        {
            float hx = size.x * 0.5f + outset;
            float hz = size.y * 0.5f + outset;
            float r  = Mathf.Clamp(radius + outset, 0f, Mathf.Min(hx, hz) - 1e-5f);
            int   n  = Mathf.Max(1, seg);

            var pts = new List<Vector2>(4 * (n + 1));

            // 四角圆心：+X+Z 起，逆时针
            var cc = new[]
            {
                new Vector2(  hx - r,  hz - r),
                new Vector2(-(hx - r), hz - r),
                new Vector2(-(hx - r), -(hz - r)),
                new Vector2(  hx - r, -(hz - r)),
            };

            for (int k = 0; k < 4; k++)
            {
                for (int i = 0; i <= n; i++)
                {
                    float a = (90f * k + 90f * i / n) * Mathf.Deg2Rad;
                    pts.Add(new Vector2(cc[k].x + Mathf.Cos(a) * r,
                                        cc[k].y + Mathf.Sin(a) * r));
                }
            }
            return pts;
        }

        /// <summary>轮廓总长（米），用来给 UV / 顶点色做周长归一。</summary>
        public static float Perimeter(List<Vector2> pts)
        {
            float sum = 0f;
            for (int i = 0; i < pts.Count; i++)
                sum += Vector2.Distance(pts[i], pts[(i + 1) % pts.Count]);
            return sum;
        }
    }
}
