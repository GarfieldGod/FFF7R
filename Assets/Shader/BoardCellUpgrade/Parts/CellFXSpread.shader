Shader "FFF7/VFX/CellFX/Spread"
{
    // 部件 4 · 平面扩散。一圈**卡牌形状**的框，从卡牌边缘往外推。
    //
    // 外扩用的是"圆角矩形整体偏移"：半宽 + off、圆角 + off。
    // 这两个同时加才是真正的外扩（只加半宽会把圆角拉直，只加半径会鼓出来）。
    //
    // 随时间变化的两个量都在这里：
    //   _WidthStart → _WidthEnd      线宽（往外推的同时变细）
    //   _AlphaStart → _AlphaEnd      透明度（默认 1 → 0，边推边化掉）
    //
    // 纯加色（a = 0），所谓"透明度"是乘在颜色上的强度。
    Properties
    {
        // 材质上只留「整套皮肤共享」的：颜色、亮度。
        // 形状 / 端点（宽度、透明度）/ 节奏由组件（CellFXSpread）用 MPB 写入，下面隐藏。
        [Header(Appearance)]
        _SpreadColor    ("扩散颜色",       Color) = (0.55, 0.92, 1.00, 1)
        _SpreadGain     ("扩散亮度",       Range(0, 4))     = 2.20

        [Header(Common)]
        _Tint           ("色调",           Color) = (1, 1, 1, 1)
        _Intensity      ("整体亮度",       Range(0.05, 4))  = 1.00
        _Level          ("等级 0-3",       Range(0, 3))     = 1
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest ("深度测试（3D 用 4）", Float) = 4

        // ---- 以下由 CellFXSpread.cs 通过 MPB 写入，材质上不显示 ----
        [HideInInspector] _Reach      ("扩散到多远",   Range(0.02, 0.7)) = 0.45
        [HideInInspector] _WidthStart ("起始宽度",     Range(0.002, 0.15)) = 0.050
        [HideInInspector] _WidthEnd   ("末尾宽度",     Range(0.002, 0.15)) = 0.014
        [HideInInspector] _AlphaStart ("起始透明度",   Range(0, 1.5))   = 1.00
        [HideInInspector] _AlphaEnd   ("末尾透明度",   Range(0, 1.5))   = 0.00
        [HideInInspector] _FadeIn     ("刚发出时的淡入", Range(0.001, 0.3)) = 0.05
        [HideInInspector] _Progress   ("进度",         Range(0, 1))     = 0
        [HideInInspector] _HalfW      ("卡牌半宽",     Float) = 0.16
        [HideInInspector] _HalfD      ("卡牌半深",     Float) = 0.22
        [HideInInspector] _Radius     ("圆角半径",     Float) = 0.0174
        [HideInInspector] _QuadW      ("Quad 宽",      Float) = 0.32
        [HideInInspector] _QuadH      ("Quad 高",      Float) = 0.44
        [HideInInspector] _Unit       ("卡牌短边",     Float) = 0.32
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
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

            float4 _SpreadColor, _Tint;
            float _SpreadGain, _Reach, _WidthStart, _WidthEnd, _AlphaStart, _AlphaEnd, _FadeIn;
            float _Progress, _HalfW, _HalfD, _Radius, _QuadW, _QuadH, _Unit;
            float _Intensity, _Level;

            float RoundBox(float2 p, float2 halfSize, float r)
            {
                float2 q = abs(p) - (halfSize - r);
                return length(max(q, 0.0)) + min(max(q.x, q.y), 0.0) - r;
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv  = v.uv;
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float t = saturate(_Progress);

                // 圆角矩形整体外扩：半宽和圆角一起加，才是真的往外偏移
                float  off  = _Reach * _Unit * t;
                float2 half = float2(_HalfW, _HalfD) + off;
                float  r    = _Radius + off;

                float2 q = (i.uv - 0.5) * float2(_QuadW, _QuadH);
                float  d = RoundBox(q, half, r);

                float sig  = max(lerp(_WidthStart, _WidthEnd, t) * _Unit * 0.5, 1e-5);
                float band = exp(-(d * d) / (sig * sig));

                float a  = lerp(_AlphaStart, _AlphaEnd, t)
                         * smoothstep(0.0, max(_FadeIn, 1e-4), t);

                float lv = saturate(_Level / 3.0);
                float3 col = _SpreadColor.rgb * (band * _SpreadGain * a);
                col *= _Intensity * (1.0 + 0.20 * lv);
                col *= _Tint.rgb;

                return float4(col, 0.0);
            }
            ENDCG
        }
    }

    Fallback Off
}
