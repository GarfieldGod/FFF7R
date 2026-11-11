Shader "FFF7/VFX/CardBan"
{
    // 卡牌禁止 —— 卡面中央一个「禁止」标（圆环 + 一道斜杠），整张卡轻微压暗。
    //
    // 为什么不用一张 PNG：
    //   ① 形状是纯几何（圆环 + 直条），两个 SDF 就画完，任意距离都锐利；
    //      出图反而要出多套尺寸，缩放糊边、mipmap 发灰。
    //   ② 卡牌是 0.32 × 0.44 / 圆角 0.0174。图铺满整卡就必须自带 alpha 圆角，
    //      否则四角方角会压在卡外；改符号大小 / 改卡尺寸都要重新出图。
    //   ③ 这个状态通常要轻微"呼吸"（否则像贴纸贴死在卡上），单图做不了。
    //
    // 混合方式的唯一重点：**这层叠在卡面（亮底）上，不能用加色**。
    //   加色会让红符号在红色卡背上变成粉、在白色人物上几乎看不见。
    //   所以走预乘 over（Blend One OneMinusSrcAlpha）：
    //     · 实心符号参与 a —— 会遮住下面的画面，读作"盖上去的标"
    //     · 外侧辉光 a = 0 —— 只加光，不遮挡，浅底上也不会切出一圈硬边
    //   a 与 rgb 必须同源衰减（aSym = core × 强度，rgb = 颜色 × aSym），
    //   否则 _On / 呼吸归零之后会留一层"透明但仍在遮挡"的黑影。
    //
    // 符号 = smin(圆环, 斜杠)：
    //   斜杠先裁到 lq ≤ R（环的外半径）之内，端点才正好被环"接住"。
    //   不裁的话它会从环里伸出去一截，看着像两根交叉的棍子而不是禁止标。
    //
    // 坐标口径：**本地米**。quad 网格本身就是目标尺寸（Transform 恒 1,1,1），
    // 所以 i.uv 和顶点本地坐标一致，_QuadW / _HalfW 都是米。
    Properties
    {
        // 材质上只留「同类卡牌共享」的：颜色、亮度。
        // 形状 / 粗细 / 角度 / 呼吸由组件（CardBanFX）用 MPB 写入，材质上不显示 ——
        // 那样同一个材质可以被 N 张卡各用各的参数。
        [Header(Appearance)]
        _BanColor       ("符号颜色",       Color)           = (0.58, 0.05, 0.09, 1)
        _BanGain        ("符号亮度",       Range(0, 3))     = 1.00

        [Header(Common)]
        _Tint           ("色调",           Color)           = (1, 1, 1, 1)
        _Intensity      ("整体亮度",       Range(0.05, 4))  = 1.00
        _Level          ("等级 0-3",       Range(0, 3))     = 1
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest ("深度测试（3D 用 4）", Float) = 4

        // ---- 以下由 CardBanFX.cs 通过 MPB 写入，材质上不显示 ----
        [HideInInspector] _On        ("开关 0-1",           Range(0, 1))       = 1
        [HideInInspector] _SymSize   ("符号直径（× 短边）",  Float)             = 0.52
        [HideInInspector] _Thick     ("环厚（× 直径）",      Float)             = 0.085
        [HideInInspector] _BarScale  ("斜杠厚度（× 环厚）",  Float)             = 1.00
        [HideInInspector] _SlashAngle("斜杠角度（度）",      Float)             = 42
        [HideInInspector] _Opacity   ("符号不透明度",        Range(0, 1))       = 0.85
        [HideInInspector] _Dim       ("整卡压暗",            Range(0, 1))       = 0.12
        [HideInInspector] _Glow      ("辉光宽度（× 短边）",  Float)             = 0.045
        [HideInInspector] _GlowGain  ("辉光亮度",            Float)             = 0.40
        [HideInInspector] _Center    ("符号偏移（× 短边）",  Vector)            = (0, 0, 0, 0)
        [HideInInspector] _Pulse     ("呼吸幅度",            Range(0, 1))       = 0.10
        [HideInInspector] _Period    ("呼吸周期（秒）",      Float)             = 2.40
        [HideInInspector] _HalfW     ("卡牌半宽",            Float)             = 0.16
        [HideInInspector] _HalfD     ("卡牌半深",            Float)             = 0.22
        [HideInInspector] _Corner    ("卡牌圆角",            Float)             = 0.0174
        [HideInInspector] _QuadW     ("Quad 宽",             Float)             = 0.384
        [HideInInspector] _QuadH     ("Quad 高",             Float)             = 0.528
        [HideInInspector] _Unit       ("卡牌短边",           Float)             = 0.32
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Cull Off
        ZWrite Off
        ZTest [_ZTest]
        // 预乘 over。单张 quad 只会被画一次，Cull Off 不会翻倍叠加。
        Blend One OneMinusSrcAlpha

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

            float4 _BanColor, _Tint, _Center;
            float _BanGain, _On, _SymSize, _Thick, _BarScale, _SlashAngle;
            float _Opacity, _Dim, _Glow, _GlowGain, _Pulse, _Period;
            float _HalfW, _HalfD, _Corner, _QuadW, _QuadH, _Unit;
            float _Intensity, _Level;

            float RoundBox(float2 p, float2 halfSize, float r)
            {
                float2 q = abs(p) - (halfSize - r);
                return length(max(q, 0.0)) + min(max(q.x, q.y), 0.0) - r;
            }

            // 圆滑并集：环和斜杠的接缝会自然收出一个内圆角
            float SMin(float a, float b, float k)
            {
                float h = clamp(0.5 + 0.5 * (b - a) / max(k, 1e-6), 0.0, 1.0);
                return lerp(b, a, h) - k * h * (1.0 - h);
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
                float on = saturate(_On);
                if (on <= 1e-4) return float4(0, 0, 0, 0);

                float2 q  = (i.uv - 0.5) * float2(_QuadW, _QuadH);
                float2 hs = float2(_HalfW, _HalfD);
                float2 hq = float2(_QuadW, _QuadH) * 0.5;

                float unit = max(_Unit, 1e-4);

                // 卡牌 SDF（负 = 卡内）。符号一律乘 inside，保证绝不溢出到卡外 ——
                // 符号碰到卡边时被卡的轮廓裁掉，而不是画到棋盘上。
                float d      = RoundBox(q, hs, _Corner);
                float inside = saturate(-d / (0.002 * unit));

                // quad 四边硬压 0：辉光漏到 quad 边界会切出一条直边
                float2 ef     = 1.0 - smoothstep(hq - 0.10 * unit, hq, abs(q));
                float  edge   = ef.x * ef.y;

                // ---- 符号几何 ----
                float2 c  = q - _Center.xy * unit;
                float  R  = max(_SymSize, 1e-3) * unit * 0.5;    // 环外半径
                float  T  = R * 2.0 * max(_Thick, 1e-3);         // 环厚
                float  lq = length(c);

                float sdRing = abs(lq - (R - T * 0.5)) - T * 0.5;

                // 斜杠：_SlashAngle 是**斜杠自身的方向**（度），法向与它垂直。
                //   42  ≈ "/"（参考图那个方向）
                //   -42 ≈ "\"（标准禁止符）
                float  ar   = _SlashAngle * 0.01745329;
                float2 bn   = float2(-sin(ar), cos(ar));
                float  bh   = T * 0.5 * max(_BarScale, 1e-3);
                float  sdBar = abs(dot(c, bn)) - bh;

                // 裁到环外沿之内 —— 端点正好落在环上。不裁就会从环里伸出去一截。
                sdBar = max(sdBar, lq - R);

                float sd = SMin(sdRing, sdBar, T * 0.55);

                // 抗锯齿：固定软边兜底 + 屏幕导数。相机拉远时自动变软，不会闪。
                float soft = max(0.0012 * unit, fwidth(sd));
                float core = saturate(0.5 - sd / soft);
                core = core * core * (3.0 - 2.0 * core);

                // 辉光只往外（max(sd,0)）：环内部保持干净，否则整块读成"发光的圆盘"
                float glow = exp(-max(sd, 0.0) / max(_Glow * unit, 1e-5));

                // ---- 合成（预乘 over）----
                float lv    = saturate(_Level / 3.0);
                float k     = _BanGain * _Intensity * (1.0 + 0.20 * lv);
                float pulse = 1.0 - saturate(_Pulse) * 0.5 * (1.0 - cos(_Time.y * 6.28318 / max(_Period, 0.05)));

                float  aSym = saturate(core * saturate(_Opacity) * k * pulse * inside * edge);
                float  aDim = saturate(_Dim) * inside * edge;
                float3 sym  = _BanColor.rgb * _Tint.rgb;

                // 暗层在下面，符号盖在上面（符号本身不该被暗层再压一次）
                float  a   = aSym + aDim * (1.0 - aSym);
                float3 rgb = sym * aSym;                                  // 预乘：rgb 与 a 同源
                rgb += sym * (glow * _GlowGain * k * pulse * inside * edge);  // 辉光只加色，不进 a

                rgb *= on;
                a   *= on;

                return float4(rgb, a);
            }
            ENDCG
        }
    }

    Fallback Off
}
