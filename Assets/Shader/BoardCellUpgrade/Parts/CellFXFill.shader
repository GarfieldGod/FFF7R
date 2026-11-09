Shader "FFF7/VFX/CellFX/Fill"
{
    // 部件 1 · 卡牌实心。
    //
    // 形状是卡牌那个圆角矩形（0.32 × 0.44，r 0.0174），在片元里用**有符号距离**裁出来：
    // 比例和圆角都是解析的，放大多少倍都不会出棱，也不需要为圆角细分网格。
    //
    // 它只管内部那一片亮面。边界那圈更亮的带是另一个部件（CellFXEdge）——
    // 拆开之后两层的亮暗节奏可以各走各的。
    //
    // 纯加色（a = 0）：只照亮卡牌和格子，不遮挡任何东西，也不会把背景压暗。
    Properties
    {
        // 材质上只留「整套皮肤共享」的：颜色、亮度。
        // 形状 / 端点 / 节奏这些「每个实例可能不同」的，全部由组件（CellFXFill）用 MPB 写入，
        // 并在下面隐藏 —— 否则同一个 .mat 被多份部件共用时，改一个全都变。
        [Header(Appearance)]
        _FillColor      ("实心颜色",       Color) = (0.10, 0.45, 1.00, 1)
        _FillGain       ("实心亮度",       Range(0, 3))     = 0.42

        [Header(Common)]
        _Tint           ("色调",           Color) = (1, 1, 1, 1)
        _Intensity      ("整体亮度",       Range(0.05, 4))  = 1.00
        _Level          ("等级 0-3",       Range(0, 3))     = 1
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest ("深度测试（3D 用 4）", Float) = 4

        // ---- 以下全部由 CellFXFill.cs 通过 MPB 写入，材质上不显示 ----
        // 默认值只是兜底（组件要是没驱动，至少还能看出个形）。
        [HideInInspector] _EdgeSoft  ("边缘软度",       Range(0.001, 0.06)) = 0.012
        [HideInInspector] _FadeIn    ("淡入占进度",     Range(0.001, 0.5)) = 0.10
        [HideInInspector] _FadeOut   ("开始熄灭的进度", Range(0.2, 1))   = 0.68
        [HideInInspector] _PulseDepth("闪烁深度",       Range(0, 1))     = 0.00
        [HideInInspector] _PulseSpeed("闪烁次数",       Float)           = 3.90
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
        Blend One One          // 纯加色：不写 a，永远不会压暗背景

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

            float4 _FillColor, _Tint;
            float _FillGain, _EdgeSoft;
            float _FadeIn, _FadeOut, _PulseDepth, _PulseSpeed;
            float _Progress, _HalfW, _HalfD, _Radius, _QuadW, _QuadH, _Unit;
            float _Intensity, _Level;

            // 圆角矩形有符号距离：d < 0 在里面，d = 0 正好在卡牌轮廓上
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

                float soft   = max(_EdgeSoft * _Unit, 1e-5);
                float inside = saturate(-d / soft);

                // 存在度：淡入 → 常量 → 熄灭。PulseDepth 默认 0，所以"常量"是默认行为。
                float pres  = smoothstep(0.0, max(_FadeIn, 1e-4), _Progress)
                            * (1.0 - smoothstep(_FadeOut, 1.0, _Progress));
                float pulse = 1.0 - _PulseDepth * (0.5 + 0.5 * sin(_Progress * _PulseSpeed * 6.2831853));

                float lv = saturate(_Level / 3.0);
                float3 col = _FillColor.rgb * (inside * _FillGain);
                col *= pres * pulse * _Intensity * (1.0 + 0.20 * lv);
                col *= _Tint.rgb;

                return float4(col, 0.0);                   // a = 0，纯加色
            }
            ENDCG
        }
    }

    Fallback Off
}
