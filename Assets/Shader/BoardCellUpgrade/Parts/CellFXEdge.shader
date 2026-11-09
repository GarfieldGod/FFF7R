Shader "FFF7/VFX/CellFX/Edge"
{
    // 部件 2 · 卡牌边框（那一条更亮的边）。
    //
    // d = 0 正好是卡牌轮廓 —— 也就是光墙的墙根。Fill 的边界、这里的亮带、Wall 的墙根
    // 在空间上是同一条线，配色要一起定，否则会看到几圈颜色不同的亮边叠在一起。
    //
    // 亮带是**跨边界**的：一半压在卡牌边上、一半落在格子面上，
    // 所以它读起来是"这张卡在发光"，而不是"格子上画了个框"。
    //
    // 纯加色（a = 0）。
    Properties
    {
        // 材质上只留「整套皮肤共享」的：颜色、亮度。
        // 形状 / 宽度 / 节奏由组件（CellFXEdge）用 MPB 写入，下面隐藏。
        [Header(Appearance)]
        _EdgeColor      ("边框颜色",       Color) = (0.80, 0.97, 1.00, 1)
        _EdgeGain       ("边框亮度",       Range(0, 4))     = 1.40

        [Header(Common)]
        _Tint           ("色调",           Color) = (1, 1, 1, 1)
        _Intensity      ("整体亮度",       Range(0.05, 4))  = 1.00
        _Level          ("等级 0-3",       Range(0, 3))     = 1
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest ("深度测试（3D 用 4）", Float) = 4

        // ---- 以下由 CellFXEdge.cs 通过 MPB 写入，材质上不显示 ----
        [HideInInspector] _EdgeWidth ("边框宽度",       Range(0.004, 0.2)) = 0.050
        [HideInInspector] _FadeIn    ("淡入占进度",     Range(0.001, 0.5)) = 0.08
        [HideInInspector] _FadeOut   ("开始熄灭的进度", Range(0.2, 1))   = 0.70
        [HideInInspector] _PulseDepth("呼吸深度",       Range(0, 1))     = 0.00
        [HideInInspector] _PulseSpeed("呼吸次数",       Float)           = 2.60
        [HideInInspector] _Progress  ("进度",           Range(0, 1))     = 0
        [HideInInspector] _HalfW     ("卡牌半宽",       Float) = 0.16
        [HideInInspector] _HalfD     ("卡牌半深",       Float) = 0.22
        [HideInInspector] _Radius    ("圆角半径",       Float) = 0.0174
        [HideInInspector] _QuadW     ("Quad 宽",        Float) = 0.32
        [HideInInspector] _QuadH     ("Quad 高",        Float) = 0.44
        [HideInInspector] _Unit      ("卡牌短边",       Float) = 0.32
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

            float4 _EdgeColor, _Tint;
            float _EdgeGain, _EdgeWidth;
            float _FadeIn, _FadeOut, _PulseDepth, _PulseSpeed;
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
                float2 q = (i.uv - 0.5) * float2(_QuadW, _QuadH);
                float  d = RoundBox(q, float2(_HalfW, _HalfD), _Radius);

                // 高斯亮带，中心在卡牌轮廓上，内外对称。
                // 宽度是"整个带子看起来多宽"，所以 sigma 取一半。
                float sig  = max(_EdgeWidth * _Unit * 0.5, 1e-5);
                float band = exp(-(d * d) / (sig * sig));

                float pres  = smoothstep(0.0, max(_FadeIn, 1e-4), _Progress)
                            * (1.0 - smoothstep(_FadeOut, 1.0, _Progress));
                float pulse = 1.0 - _PulseDepth * (0.5 + 0.5 * sin(_Progress * _PulseSpeed * 6.2831853));

                float lv = saturate(_Level / 3.0);
                float3 col = _EdgeColor.rgb * (band * _EdgeGain);
                col *= pres * pulse * _Intensity * (1.0 + 0.25 * lv);
                col *= _Tint.rgb;

                return float4(col, 0.0);
            }
            ENDCG
        }
    }

    Fallback Off
}
