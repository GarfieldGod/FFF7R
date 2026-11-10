Shader "FFF7/VFX/CardAura"
{
    // 卡牌辉芒 —— 卡牌边缘向四周**持续**发散的光，带扰动。
    //
    // 三层叠出来：
    //   ① 贴边亮线 rim   ：压在卡牌轮廓上的一圈细亮线，内外各透一点（读作"卡边在发光"）
    //   ② 贴边柔光 halo  ：对称的一圈柔光，把卡牌从棋盘上"托"起来
    //   ③ 外发光芒 rays  ：从卡边往外的长条光，按**角度**分布，越远越淡
    //      三个"收着点"的旋钮：
    //        _Offset       起点离卡边多远才开始有光（中间留一圈空）
    //        _StartAlpha   光条刚出现那一点有多亮（< 1 = 薄雾，= 1 = 贴边一道亮边）
    //        _EdgeIrr      外边缘不规则度（0 = 外沿与卡框**同形**，1 = 每条长短不一）
    //
    // "外边缘和卡框一样"是怎么做到的：
    //   光条的长度按**卡牌 SDF 的距离**量，所以只要所有光条都停在同一个距离上，
    //   末端自然就落成一条与卡牌轮廓等距的圆角矩形 —— 形状与卡框完全一致。
    //   默认那个"长短不一"只是把每条乘上 lenVar 噪声，乘以 (1 - _EdgeIrr) 就能收干净。
    //
    // "有变化、像被扰动"全部来自 3 层**圆上采样**的值噪声：
    //   角度 θ → dir = (cosθ, sinθ) → 在噪声空间里绕一圈采样。
    //   绕圈采样天然是 2π 周期的，所以 θ = ±π 那根接缝上不会有硬线。
    //   （拿 atan2 的结果直接喂 1D 噪声，就会在 -X 轴切出一条笔直的缝 ——
    //    这是这类"放射状"特效最常见的翻车点，任何角度场都必须用 dir 采样。）
    //
    // 域扭曲（warp）：把**采样点本身**推歪，光芒才会断、会粗细不匀、会缓慢流动。
    // 只调亮度是调不出"扰乱"的，必须动坐标。
    //
    // 时间来自 _Time.y，自己走，不接进度条 —— 这个特效是"状态"（可选 / 已激活），
    // 挂上就一直发散；开关由 _On 平滑（C# 里 MoveTowards）。
    //
    // 纯加色（a = 0），所谓"透明度"是乘在颜色上的强度。
    //
    // 坐标口径：全部是**本地米**。网格是 1×1 的 quad，尺寸靠 Transform.localScale 给，
    // 所以 shader 里的 _QuadW / _HalfW 和顶点本地坐标一致，父级缩放会整体等比放大。
    Properties
    {
        // 材质上只留"整套皮肤共享"的：颜色、亮度。
        // 形状 / 条数 / 扰动 / 速度由组件（CardAuraFX）用 MPB 写入，材质上不显示。
        [Header(Appearance)]
        _AuraColor      ("光芒颜色",       Color)           = (0.34, 0.74, 1.00, 1)
        _AuraGain       ("光芒亮度",       Range(0, 4))     = 1.00

        [Header(Common)]
        _Tint           ("色调",           Color)           = (1, 1, 1, 1)
        _Intensity      ("整体亮度",       Range(0.05, 4))  = 1.00
        _Level          ("等级 0-3",       Range(0, 3))     = 1
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest ("深度测试（3D 用 4）", Float) = 4

        // ---- 以下由 CardAuraFX.cs 通过 MPB 写入，材质上不显示 ----
        [HideInInspector] _On         ("开关 0-1",         Range(0, 1))      = 1
        [HideInInspector] _RayCount   ("光芒条数",         Range(6, 160))    = 30
        [HideInInspector] _Reach      ("发散距离（× 短边）", Float)           = 0.20
        [HideInInspector] _Offset     ("起始偏移（× 短边）", Range(0, 0.5))  = 0.00
        [HideInInspector] _StartAlpha ("起始强度",         Range(0, 1))      = 0.60
        [HideInInspector] _EdgeIrr    ("边缘不规则度",     Range(0, 1))      = 1.00
        [HideInInspector] _Turbulence ("扰动量",           Range(0, 1.5))    = 0.45
        [HideInInspector] _Bend       ("弯曲量",           Range(0, 6))      = 1.60
        [HideInInspector] _BendS      ("S 形弯曲",         Range(0, 6))      = 1.00
        [HideInInspector] _Flow       ("流动速度",         Range(0, 4))      = 1.00
        [HideInInspector] _Spin       ("旋转速度（度/秒）", Range(-40, 40))   = 5
        [HideInInspector] _Falloff    ("衰减指数",         Range(0.5, 5))    = 1.05
        [HideInInspector] _RimWidth   ("边缘亮线宽度",     Range(0.004, 0.12)) = 0.022
        [HideInInspector] _RimGain    ("边缘亮线亮度",     Range(0, 3))      = 0.55
        [HideInInspector] _HaloGain   ("贴边柔光",         Range(0, 3))      = 0.32
        [HideInInspector] _HalfW      ("卡牌半宽",         Float)             = 0.16
        [HideInInspector] _HalfD      ("卡牌半深",         Float)             = 0.22
        [HideInInspector] _Radius     ("圆角半径",         Float)             = 0.0174
        [HideInInspector] _QuadW      ("Quad 宽",          Float)             = 0.544
        [HideInInspector] _QuadH      ("Quad 高",          Float)             = 0.748
        [HideInInspector] _Unit       ("卡牌短边",         Float)             = 0.32
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

            float4 _AuraColor, _Tint;
            float _AuraGain, _On, _RayCount, _Reach, _Turbulence, _Flow, _Spin;
            float _Offset, _StartAlpha, _EdgeIrr;
            float _Bend, _BendS;
            float _Falloff, _RimWidth, _RimGain, _HaloGain;
            float _HalfW, _HalfD, _Radius, _QuadW, _QuadH, _Unit;
            float _Intensity, _Level;

            float RoundBox(float2 p, float2 halfSize, float r)
            {
                float2 q = abs(p) - (halfSize - r);
                return length(max(q, 0.0)) + min(max(q.x, q.y), 0.0) - r;
            }

            // ---- 值噪声 --------------------------------------------------
            float Hash21(float2 p)
            {
                p = frac(p * float2(127.1, 311.7));
                p += dot(p, p + 34.56);
                return frac(p.x * p.y);
            }

            float N2(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                float a = Hash21(i);
                float b = Hash21(i + float2(1.0, 0.0));
                float c = Hash21(i + float2(0.0, 1.0));
                float d = Hash21(i + float2(1.0, 1.0));
                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
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

                float d = RoundBox(q, hs, _Radius);       // 负 = 卡内，正 = 卡外（米）
                float dist = max(d, 0.0);

                float tm = _Time.y * _Flow;

                // ---- ① 角度场：在噪声空间里绕圈采样（天然无接缝）-------
                float  th   = atan2(q.y, q.x) + _Spin * tm * 0.01745329;
                float2 dir0 = float2(cos(th), sin(th));

                // ---- ①' 弯曲：让**角位置随半径漂移** → 光条变曲线 -------
                // 光条的位置由噪声等值线 n(θ_eff) = const 决定，而 θ_eff = θ + bend(r)；
                // 所以 bend 变多少弧度，光条在角向就摆多少。这是唯一的差别所在：
                // 原来 bend 恒 0 → 同一角度上所有半径都是同一条光，笔直射出去；
                // 加了 bend(r) 之后同一条光的 θ 随 r 变 → 曲线。
                //   · 一次项 ∝ r  ：整根往一侧弯（被风吹的感觉）
                //   · 二次项 ∝ r² ：远处翻回来 → S 形
                // 两个都设 0 就退回原来的直射线。
                float  br = dist / max(_Unit, 1e-4);
                float  w1 = (N2(dir0 * 1.25 + float2(3.3, tm * 0.09)) - 0.5) * 2.0;
                float  w2 = (N2(dir0 * 3.10 + float2(19.7, -tm * 0.13)) - 0.5) * 2.0;
                float  thb = th + _Bend * br * w1 + _BendS * br * br * w2;
                float2 dir = float2(cos(thb), sin(thb));

                // 噪声半径 = 条数 / 2π —— 绕一圈正好跨过「条数」个噪声格
                float  R  = max(_RayCount, 4.0) * 0.15915494;
                float2 np = dir * R;

                // 域扭曲：把采样点推歪。这才是"扰乱"的来源（不是调亮度）
                float2 warp = float2(N2(np * 0.55 + float2(tm * 0.11, 3.1)),
                                     N2(np * 0.55 + float2(17.7, -tm * 0.09))) - 0.5;
                np += warp * (_Turbulence * R * 1.6);

                float n = N2(np + float2(tm * 0.31, -tm * 0.18)) * 0.58
                        + N2(np * 2.13 + float2(-tm * 0.44, tm * 0.24) + 11.3) * 0.29
                        + N2(np * 4.37 + float2(tm * 0.62, tm * 0.31) + 27.9) * 0.13;

                float spike = smoothstep(0.24, 0.82, n);
                float streak = 0.22 + 0.62 * pow(spike, 1.3);   // 留一点底色，不然只剩"毛边"

                // ---- ② 径向分布：从卡边（或 offset 处）往外，到 reach 归零 --------
                float margin = max(min(hq.x - _HalfW, hq.y - _HalfD), 1e-4);
                // 夹到 quad 余量以内：那样无论 _Reach 调多大，都不会在 quad 边界被切断
                float reach  = clamp(_Reach * _Unit, 1e-4, margin * 0.92);
                // 每条光芒长短不一。不给这个的话所有光芒都停在同一半径上，看起来像个"壳"
                float lenVar = 0.55 + 0.90 * N2(dir * 2.4 + float2(tm * 0.06, 7.7));

                // 边缘不规则度：
                //   1 = 每条光自己的长度（默认，末端毛糙、野生的光芒）
                //   0 = 全部拉回 1.0 → 末端都落在 SDF 距离 = reach 的等距线上，
                //       也就是一条与卡牌轮廓同形（等距外扩）的圆角矩形。
                // 之所以能这么"整"，是因为长度量的就是卡牌 SDF 距离，不是极坐标半径 ——
                // 换成半径的话，同样的末端距离在角上会跑得比边上远，形状就不是卡框了。
                float irr    = saturate(_EdgeIrr);
                lenVar = lerp(1.0, lenVar, irr);

                float r2     = min(reach * lenVar, margin * 0.95);

                // 起始偏移：光条不从卡边直接冒出来，而是离开 offset 之后才开始（0 = 贴边起）。
                // 这一段（卡边 → offset）只有 rim / halo，读作"光从卡外一圈浮起来"。
                float off  = max(_Offset, 0.0) * _Unit;
                float span = max(r2 - off, 0.02 * _Unit);   // offset 超过长度时留一条窄带，别出负值
                float t01  = saturate((dist - off) / span); // 0 = 起点，1 = 末端

                float radial = pow(1.0 - t01, _Falloff);

                // 起始强度：光条刚出现那一点的亮度。1 = 起点最亮（旧行为），
                // 0.3 = 从几乎透明慢慢亮起来 —— 前者贴边是一道"亮边"，后者是一层"薄雾"。
                radial *= lerp(_StartAlpha, 1.0, smoothstep(0.0, 0.45, t01));

                // offset 之前一条光都不写。只在 off > 0 时才需要这道门：
                // off = 0 时让 gate 恒为 1，保证和旧参数完全一致。
                // 羽化放在 offset **之内**（off-2soft → off），这样 dist = off 处 gate 正好 = 1，
                // "起始强度"就是光条真正开始那一刻的亮度，不会被门又砍一半。
                float soft = 0.006 * _Unit;
                radial *= (off > 1e-5) ? smoothstep(off - 2.0 * soft, off, dist) : 1.0;

                // 沿光芒长度方向的起伏（用 dir 采样 → 同样无接缝）
                float fl = 0.70 + 0.55 * N2(dir * 4.0 +
                                            float2(dist / max(_Unit, 1e-4) * 11.0 - tm * 0.85, 5.3));

                // 卡内不写光芒，卡边处软进来（不切一条硬缝）
                float outMask = smoothstep(-0.025 * _Unit, 0.025 * _Unit, d);

                // 边缘不规则度的第二半：**末端填平**。
                // 只统一长度是不够的 —— 光条之间本来就有暗缝，缺口处照样没光，
                // 外沿读起来还是一圈碎齿。所以按 (1 - _EdgeIrr) 把靠近末端那一带的
                // 角向起伏平掉（趋向 streak×fl 的角向均值 0.46，不额外提亮）：
                // 整圈亮度同步衰减到 0，可见的外沿才是一条连续的、与卡框同形的线。
                // 只在 t01 后半段起作用，内圈的光条形状一点不动。
                float tipFlat = (1.0 - irr) * smoothstep(0.38, 1.0, t01);
                float amp     = lerp(streak * fl, 0.46, tipFlat);

                // ---- ③ 贴边亮线 + 柔光 ---------------------------------
                float rimSig = max(_RimWidth * _Unit * 0.5, 1e-5);
                float rim    = exp(-(d * d) / (rimSig * rimSig));
                float halo   = exp(-abs(d) / max(0.055 * _Unit, 1e-5));

                float m = rim * _RimGain + halo * _HaloGain + amp * radial * outMask;

                // quad 边上硬压到 0：辉光往外漏到 quad 边界会切出一条直边（沿哪条边都难看）。
                // 这一段只在最后 0.12 个卡牌短边内起作用，正常参数下正好在 reach 之外。
                float2 ef = 1.0 - smoothstep(hq - 0.12 * _Unit, hq, abs(q));
                m *= ef.x * ef.y;

                float lv = saturate(_Level / 3.0);
                float3 col = _AuraColor.rgb * (m * _AuraGain);
                col *= on * _Intensity * (1.0 + 0.20 * lv);
                col *= _Tint.rgb;

                return float4(col, 0.0);
            }
            ENDCG
        }
    }

    Fallback Off
}
