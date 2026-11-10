Shader "FFF7/VFX/CellFX/Surge"
{
    // 部件 6 · 内涌。从**卡牌中心**扩散到**卡牌边缘**，到边就停，不出卡。
    //
    // 和另外两个平面部件的区别：
    //   Fill    整张卡一起亮，没有起点
    //   Spread  从卡边往**外**推，越推越淡
    //   Surge   从卡心往**边缘**推，推到边缘为止（本文）
    //
    // 波前的算法：**卡牌的小号副本**。把坐标除以 s（s: 0 → 1），用同一套圆角矩形 SDF
    // 量距离。s = 1 时波前正好压在卡牌轮廓上，四条边同时到 —— 不是"先到短边、
    // 再到长边"那种歪掉的波（那需要按到边的距离场做，卡是 8:11，会很怪）。
    // dScaled × s 就是到波前的**真实距离**（米），负值 = 已经扫过。
    //
    // 亮度 = 波前那道亮环 + 扫过之后留下的余辉（_TailGain）：
    //   _TailGain = 0    只有一圈环从中心往外推，扫过的地方是空的
    //   _TailGain = 1    扫过的地方全亮，最后整张卡被填满
    //
    // 卡外一个像素都不写（inCard 遮掉），所以不像 Spread 那样会窜出卡边。
    // 当进度超过「冲到边」那一刻，环就停在卡边上，读作"扩散到边缘、边缘亮了一圈"。
    //
    // 纯加色（a = 0），所谓"透明度"是乘在颜色上的强度。
    Properties
    {
        // 材质上只留「整套皮肤共享」的：颜色、亮度。
        // 形状 / 端点 / 节奏由组件（CellFXSurge）用 MPB 写入，下面隐藏。
        [Header(Appearance)]
        _SurgeColor     ("内涌颜色",       Color) = (0.62, 0.94, 1.00, 1)
        _SurgeGain      ("内涌亮度",       Range(0, 4))     = 1.60

        [Header(Common)]
        _Tint           ("色调",           Color) = (1, 1, 1, 1)
        _Intensity      ("整体亮度",       Range(0.05, 4))  = 1.00
        _Level          ("等级 0-3",       Range(0, 3))     = 1
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest ("深度测试（3D 用 4）", Float) = 4

        // ---- 以下由 CellFXSurge.cs 通过 MPB 写入，材质上不显示 ----
        [HideInInspector] _Sweep      ("冲到卡边占的进度", Range(0.2, 1))     = 0.70
        [HideInInspector] _FrontWidth ("波前宽度",         Range(0.01, 0.30)) = 0.055
        [HideInInspector] _TailGain   ("扫过后的余辉",     Range(0, 1.5))     = 0.35
        [HideInInspector] _EdgeSoft   ("边缘软度",         Range(0.001, 0.06)) = 0.010
        [HideInInspector] _FadeIn     ("淡入占进度",       Range(0.001, 0.5)) = 0.05
        [HideInInspector] _FadeOut    ("开始熄灭的进度",   Range(0.2, 1))     = 0.86
        [HideInInspector] _Progress   ("进度",             Range(0, 1))       = 0
        [HideInInspector] _HalfW      ("卡牌半宽",         Float) = 0.16
        [HideInInspector] _HalfD      ("卡牌半深",         Float) = 0.22
        [HideInInspector] _Radius     ("圆角半径",         Float) = 0.0174
        [HideInInspector] _QuadW      ("Quad 宽",          Float) = 0.32
        [HideInInspector] _QuadH      ("Quad 高",          Float) = 0.44
        [HideInInspector] _Unit       ("卡牌短边",         Float) = 0.32
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

            float4 _SurgeColor, _Tint;
            float _SurgeGain, _Sweep, _FrontWidth, _TailGain, _EdgeSoft;
            float _FadeIn, _FadeOut;
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

                // s = 波前那个"小号卡牌"相对卡牌的倍率。冲到边缘之后一直停在 1。
                float s = clamp(t / max(_Sweep, 1e-3), 0.0, 1.0);
                s = max(s, 1e-3);                       // 起步那一帧别除零

                float2 q  = (i.uv - 0.5) * float2(_QuadW, _QuadH);
                float2 hs = float2(_HalfW, _HalfD);

                // 波前距离（米）：负 = 已经扫过，正 = 还没到
                float dReal = RoundBox(q / s, hs, _Radius) * s;

                float soft  = max(_EdgeSoft * _Unit, 1e-5);
                float inCard = saturate(-RoundBox(q, hs, _Radius) / soft);   // 卡内 = 1
                float sig   = max(_FrontWidth * _Unit * 0.5, 1e-5);

                float band  = exp(-(dReal * dReal) / (sig * sig));           // 波前亮环
                float swept = saturate(-dReal / sig);                        // 已扫过（软边同宽度）

                float m = (band + _TailGain * swept) * inCard;

                float pres = smoothstep(0.0, max(_FadeIn, 1e-4), t)
                           * (1.0 - smoothstep(_FadeOut, 1.0, t));

                float lv = saturate(_Level / 3.0);
                float3 col = _SurgeColor.rgb * (m * _SurgeGain);
                col *= pres * _Intensity * (1.0 + 0.20 * lv);
                col *= _Tint.rgb;

                return float4(col, 0.0);
            }
            ENDCG
        }
    }

    Fallback Off
}
