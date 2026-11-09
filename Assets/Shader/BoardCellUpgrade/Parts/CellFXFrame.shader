Shader "FFF7/VFX/CellFX/Frame"
{
    // 部件 5 · 垂直边框。**一条**框，贴着卡牌轮廓（含四角圆弧）往上飞出去。
    //
    // 一条框一个组件：要两条就在组里挂两个 Frame，各带自己的开始时刻。
    // 这样"第二个框在第一个框 70% 处出发"在时间表里就是一行，不是写死在循环里。
    //
    // 位移和线宽都是真的动顶点（vert 里），所以它是"飞出去"，不是"墙上画了一道横纹"：
    //   pos.y = 底 + 进度 × _Rise × 单位 + uv.y × 当前线宽
    // 线宽 = _WidthStart → _WidthEnd，于是"起飞时粗、散尽时细"是几何变化，不用换网格。
    //
    // 纯加色（a = 0）。亮底上会看不见 —— 那是加色的固有限制，不是这里的问题。
    Properties
    {
        // 材质上只留「整套皮肤共享」的：颜色、亮度。
        // 高度 / 端点（宽度、透明度）/ 节奏由组件（CellFXFrame）用 MPB 写入，下面隐藏 ——
        // 关键是：Frame1 和 Frame2 共用一个 .mat，端点在材质上就只能一样，
        // 想让两条框粗细不同，必须在组件上各调各的。
        [Header(Appearance)]
        _FrameColor     ("线框颜色",       Color) = (0.80, 0.98, 1.00, 1)
        _FrameGain      ("线框亮度",       Range(0, 4))     = 0.80

        [Header(Common)]
        _Tint           ("色调",           Color) = (1, 1, 1, 1)
        _Intensity      ("整体亮度",       Range(0.05, 4))  = 1.00
        _Level          ("等级 0-3",       Range(0, 3))     = 1
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest ("深度测试（3D 用 4）", Float) = 4

        // ---- 以下由 CellFXFrame.cs 通过 MPB 写入，材质上不显示 ----
        [HideInInspector] _WidthStart ("起始宽度",     Range(0.005, 0.4)) = 0.090
        [HideInInspector] _WidthEnd   ("末尾宽度",     Range(0.005, 0.4)) = 0.040
        [HideInInspector] _Rise       ("飞出的高度",   Range(0, 3))     = 1.30
        [HideInInspector] _AlphaStart ("起始透明度",   Range(0, 1.5))   = 1.00
        [HideInInspector] _AlphaEnd   ("末尾透明度",   Range(0, 1.5))   = 0.00
        [HideInInspector] _Soft       ("线宽内的柔和度", Range(0.08, 1))  = 0.32
        [HideInInspector] _FadeIn     ("起飞时的淡入", Range(0.001, 0.3)) = 0.08
        [HideInInspector] _Progress   ("进度",         Range(0, 1))     = 0
        [HideInInspector] _Unit       ("卡牌短边",     Float) = 0.32
        [HideInInspector] _BaseLift   ("底边高度",     Float) = 0.002
    }

    SubShader
    {
        Tags { "Queue"="Transparent+20" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Cull Off
        ZWrite Off
        ZTest [_ZTest]
        Blend One One

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv  : TEXCOORD0;
            };

            float4 _FrameColor, _Tint;
            float _FrameGain, _WidthStart, _WidthEnd, _Rise, _AlphaStart, _AlphaEnd, _Soft, _FadeIn;
            float _Progress, _Unit, _BaseLift, _Intensity, _Level;

            v2f vert(appdata v)
            {
                v2f o;

                float t = saturate(_Progress);
                // 线宽随时间变：起飞时粗、散尽时细
                float w = lerp(_WidthStart, _WidthEnd, t) * _Unit;
                // 底边跟着进度往上抬；uv.y 0..1 铺在这条宽带上 —— 整段沿 Y 平移，不往外扩
                float y = _BaseLift + v.uv.y * w + t * _Rise * _Unit;

                float3 pos = float3(v.vertex.x, y, v.vertex.z);
                o.pos = UnityObjectToClipPos(float4(pos, 1.0));
                o.uv  = v.uv;
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float t  = saturate(i.uv.y);                       // 这条框自己的上下
                float dd = (t - 0.5) / max(_Soft, 1e-3);
                float core = exp(-dd * dd);                        // 细亮芯

                float a = lerp(_AlphaStart, _AlphaEnd, saturate(_Progress))
                        * smoothstep(0.0, max(_FadeIn, 1e-4), _Progress);

                float lv = saturate(_Level / 3.0);
                float3 col = _FrameColor.rgb * (core * _FrameGain * a);
                col *= _Intensity * (1.0 + 0.30 * lv);
                col *= _Tint.rgb;

                return float4(col, 0.0);
            }
            ENDCG
        }
    }

    Fallback Off
}
