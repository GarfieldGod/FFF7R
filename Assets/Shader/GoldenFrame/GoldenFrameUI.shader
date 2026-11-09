// GoldenFrameUI —— 卡牌 / 面板高亮金框，纯程序化，零贴图
//
// 用法：Image (Source Image = None) + 本材质 + GoldenFrameUI 组件
// 渲染管线：Built-in（Unity 2022.3 LTS / UGUI）
//
// 设计单位说明：内部所有长度（线宽、间距、圆角、角标）都以"设计尺寸的 quad 高 = 0.52 世界单位"
// 为基准，运行时按 RectTransform 的实际宽高比自动缩放。所以同一套参数在任意尺寸下观感一致，
// 和 Blender 烘焙版（VFX/GoldenFrame/frames/）用的是同一组数值。

Shader "FFF7/UI/GoldenFrame"
{
    Properties
    {
        // 注意：[Header(...)] 的参数只能是不带引号的 ASCII 标识符。
        // 写中文（如 [Header(金线)]）会让 ShaderLab 直接解析失败，Shader.Find 返回 null。
        // 中文说明只能出现在属性自己的引号标签里。
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        [Header(GoldLines)]
        _OuterInset     ("外侧细线 · 内缩", Range(0, 0.05))     = 0.0030
        _OuterWidth     ("外侧细线 · 线宽", Range(0.0005, 0.02)) = 0.0026
        _MainInset      ("主金带 · 内缩",   Range(0, 0.05))     = 0.0150
        _MainWidth      ("主金带 · 线宽",   Range(0.001, 0.03)) = 0.0098
        _CoreWidth      ("白热芯 · 线宽",   Range(0, 0.01))     = 0.0016
        _Radius         ("圆角半径",        Range(0, 0.06))     = 0.0174
        _Pad            ("卡外辉光余量",    Range(0, 0.12))     = 0.0400

        [Header(Colors)]
        _OuterColor     ("外侧细线", Color) = (0.97, 0.76, 0.39, 1)
        _MainColor      ("主金带",   Color) = (1.00, 0.62, 0.14, 1)
        _CoreColor      ("白热芯",   Color) = (1.00, 0.90, 0.68, 1)
        _BracketColor   ("角标",     Color) = (1.00, 0.74, 0.34, 1)
        _Intensity      ("整体亮度", Range(0.01, 4)) = 1.15

        [Header(Halo)]
        _HaloFalloff    ("衰减速度（越大越窄）", Range(2, 160)) = 20
        _HaloPower      ("辉光强度",   Range(0, 2))  = 0.50
        _HaloInner      ("向内衰减倍数（1 = 内外一致）", Range(1, 6)) = 1.40

        [Header(Contrast)]
        // 浅色卡面上发光没有余量（白 + 金 = 白）。主要靠 _HaloCut 让金带保持饱和；
        // _RimStrength 是"还嫌不够明显"时的补充手段，代价是画面上会多一条暗边，默认关掉。
        _HaloCut        ("实线底下压掉的辉光（浅底越大越饱和）", Range(0, 1)) = 0.75
        _RimColor       ("深色描边 · 颜色", Color) = (0.055, 0.032, 0.010, 1)
        _RimWidth       ("深色描边 · 线宽（0 = 关）", Range(0, 0.03)) = 0.0035
        _RimStrength    ("深色描边 · 强度（0 = 关，会出暗边）", Range(0, 1)) = 0.00

        [Header(Brackets)]
        _BracketOffset  ("离边距离",   Range(0, 0.10))     = 0.0320
        _BracketThickness ("臂厚",     Range(0.001, 0.02)) = 0.0062
        _BracketLength  ("臂长",       Range(0.005, 0.14)) = 0.0480
        _SpurLength     ("斜刻长度",   Range(0, 0.06))     = 0.0200
        _SpurWidth      ("斜刻宽度",   Range(0, 0.01))     = 0.0016
        _SpurOffset     ("斜刻离边距离", Range(0, 0.14))   = 0.0495

        [Header(Animation)]
        // 全片只有一条时间曲线：周期由 _Speed 决定，内层明暗 / 角标收放共用它。
        _Speed          ("速度（0.5 = 2 秒一轮）", Range(0, 4)) = 0.50
        _Breathe        ("内层呼吸深度", Range(0, 0.6)) = 0.24
        _BracketMin     ("角标最小缩放", Range(0.3, 1)) = 0.74
        _BracketPress   ("角标按下增亮", Range(0, 1))   = 0.30

        [Header(SizingAuto)]
        _QuadW          ("Quad 宽 (px)", Float) = 400
        _QuadH          ("Quad 高 (px)", Float) = 520

        [Header(UIMaskAuto)]
        _StencilComp    ("Stencil Comparison", Float) = 8
        _Stencil        ("Stencil ID", Float) = 0
        _StencilOp      ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        [Enum(UnityEngine.Rendering.ColorWriteMask)] _ColorMask ("Color Mask", Float) = 15

        [Header(Depth)]
        // UGUI 用 8(Always)，和 Screen Space - Overlay 的行为一致；
        // 挂到 3D 网格 / Quad 上必须改成 4(LEqual)，否则框会盖在挡在前面的棋子和桌子上面。
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest ("深度测试 (UI=8 / 3D=4)", Float) = 8
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        // 由 _ZTest 控制：UI 面片 8(Always)，3D 网格 4(LEqual)。
        // 不再用 [unity_GUIZTestMode]，那个全局只有 Canvas 会填，网格渲染时拿到 0 = 不测深度。
        ZTest [_ZTest]
        // 预乘 alpha（"over"）：out = rgb + dst * (1 - a)。
        //
        // 原来写的是 Blend SrcAlpha One（加色）。加色**只能提亮**，所以白底上白 + 金 = 白，
        // 整个框消失；只有黑底才看得出东西。这不是参数问题，是混合模式本身的上限。
        // 改成 over 之后两种底都成立：
        //   · 亮到 1 以上（主金带、白热芯）→ a = 1，完全盖住底下的卡面，白底上也是实的金色；
        //   · 弱到 0.1（辉光外沿）        → a = 0.1，卡面基本透出来，黑底上的观感与加色完全一致
        //     （黑底时 dst = 0，out = rgb + 0，算式退化成加色）。
        // 前提是 frag 里输出的 rgb 已经是预乘过的（见 add / cover）。
        Blend One OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            #define DESIGN_H 0.52
            #define TAU 6.28318530718
            #define MIN_HALF 0.02

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex        : SV_POSITION;
                fixed4 color         : COLOR;
                float2 texcoord      : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
            };

            fixed4 _Color;
            float4 _ClipRect;

            float _OuterInset, _OuterWidth;
            float _MainInset, _MainWidth, _CoreWidth;
            float _Radius, _Pad;
            fixed4 _OuterColor, _MainColor, _CoreColor, _BracketColor;
            float _Intensity;
            float _HaloFalloff, _HaloPower, _HaloInner, _HaloCut;
            fixed4 _RimColor;
            float _RimWidth, _RimStrength;
            float _BracketOffset, _BracketThickness, _BracketLength;
            float _SpurLength, _SpurWidth, _SpurOffset;
            float _Speed, _Breathe, _BracketMin, _BracketPress;
            float _QuadW, _QuadH;

            // 圆角矩形 SDF：内部为负，边界为 0，梯度恒为 1（所以线宽在世界单位下均匀）
            float RoundedBox(float2 p, float2 halfSize, float radius)
            {
                float2 q = abs(p) - (halfSize - radius);
                return length(max(q, 0.0)) + min(max(q.x, q.y), 0.0) - radius;
            }

            // 到矩形边界的等距线：inset 为向内距离（正数），返回 0..1 的覆盖率
            float Band(float d, float inset, float width, float aa)
            {
                float halfW = max(width * 0.5, 1e-5);
                return 1.0 - smoothstep(max(halfW - aa, 0.0), halfW + aa, abs(d + inset));
            }

            // 轴对齐矩形 SDF，中心 c、半尺寸 h
            float BoxSDF(float2 p, float2 c, float2 h)
            {
                float2 q = abs(p - c) - h;
                return length(max(q, 0.0)) + min(max(q.x, q.y), 0.0);
            }

            v2f vert(appdata_t v)
            {
                v2f OUT;
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = v.texcoord;
                OUT.color = v.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                // ---- 归一化到设计单位：p.y ∈ [-0.26, 0.26]，p.x 按实际宽高比展开 ----
                float aspect = _QuadW / max(_QuadH, 1.0);
                float2 p = (IN.texcoord - 0.5) * float2(aspect, 1.0) * DESIGN_H;

                float2 halfQuad = float2(aspect, 1.0) * DESIGN_H * 0.5;
                float pad = min(_Pad, min(halfQuad.x, halfQuad.y) - MIN_HALF);
                float2 halfCard = max(halfQuad - pad, float2(MIN_HALF, MIN_HALF));

                float radius = min(_Radius, min(halfCard.x, halfCard.y));
                float d = RoundedBox(p, halfCard, radius);

                // 抗锯齿宽度跟着屏幕导数走，任何分辨率下都是一像素
                float aa = max(fwidth(d) * 0.6, 2e-5);

                // ---- 时间 ----
                // 只有一条曲线。_Speed 定周期，一轮 = 一次呼吸 + 一次角标收放。
                // 角标用 ease 收放、内层用 cos 明暗，两者同频同相：
                // 角标张到最大时最亮，收到最小时最暗，看起来是同一口气在"呼"。
                float t = _Time.y * _Speed;
                float wave = cos(TAU * t);
                float breathe = (1.0 - _Breathe) + _Breathe * wave;
                float ease = 0.5 - 0.5 * wave;
                float bs = 1.0 - (1.0 - _BracketMin) * ease;
                float press = saturate((1.0 - bs) / max(1.0 - _BracketMin, 1e-4));

                // ---- 三道金线 ----
                float lineOuter = Band(d, _OuterInset, _OuterWidth, aa);
                float lineMain = Band(d, _MainInset, _MainWidth, aa);
                float lineCore = Band(d, _MainInset, _CoreWidth, aa);

                // ---- 深色描边：默认关闭 ----
                // 它确实能让白底更"抓眼"，代价是**在光晕里切出一条暗边**（halo 被掐掉一截），
                // 任何底色上都看得到，深色卡面尤其像贴了一圈黑胶带。所以默认强度 = 0。
                // 浅底优先用 _HaloCut（见下），实在不够再开这个。
                float rimInset = _OuterInset + _OuterWidth * 0.5 + _RimWidth * 0.5;
                float rimAA = max(aa, _RimWidth * 0.35);      // 边缘做软，别像钢笔描的
                float rim = Band(d, rimInset, _RimWidth, rimAA) * _RimStrength;

                // 辉光：以主带中心为轴双向指数衰减。向内收得更快，否则整张卡面会糊一层橙雾
                // （烘焙版也有这个问题，只是被卡面的深色底盖住了，纯黑底上会非常明显）。
                float dd = d + _MainInset;
                float falloff = dd > 0.0 ? _HaloFalloff : _HaloFalloff * max(_HaloInner, 1.0);
                // 实线底下不许叠辉光：金带本来就是 HDR（1.15 倍），再叠 50% 辉光会把绿色通道
                // 顶到 1.0 → 金带变成淡黄 (1.00, 1.00, 0.25)。黑底上不明显，白底上就是"发白的细线"。
                // 压掉之后金带保持 (1.00, 0.75, 0.17) 的饱和金，白底上也一眼能看见，且不需要暗边。
                float lineSolid = max(lineOuter, max(lineMain, lineCore));
                // 辉光**只允许留在框里**：d > 0 就是卡面轮廓之外。
                // 原来这里没有上限，exp 衰减一路糊到 quad 边缘，画面外圈一大片橙雾，
                // 看起来像"边框外面还在发光"。现在按外侧细线的外沿硬切一刀：
                // 那一刀之外一个像素都不写（连 alpha 也不给），完全透明。
                float outerEdge = max(_OuterInset - _OuterWidth * 0.5, 0.0);  // 外侧细线的外沿（d 为负=框内）
                float haloMask = 1.0 - smoothstep(-outerEdge - aa, -outerEdge + aa, d);
                float halo = exp(-abs(dd) * falloff) * _HaloPower
                           * (1.0 - _HaloCut * lineSolid)
                           * (1.0 - rim)
                           * haloMask;

                // ---- 四角提示键：abs(p) 把四角镜像成一处，只算第一象限 ----
                float2 ap = abs(p);
                float2 corner = halfCard - _BracketOffset * bs;

                float tArm = _BracketThickness * bs;
                float lArm = _BracketLength * bs;
                float2 lp = ap - corner;                       // 局部坐标，肘部在 (0,0)
                float ll = max(lArm, 1e-4);

                // L 的两条臂 = 两个矩形取并集
                float s1 = BoxSDF(lp, float2(-ll * 0.5, -tArm * 0.5), float2(ll, tArm) * 0.5);
                float s2 = BoxSDF(lp, float2(-tArm * 0.5, -ll * 0.5), float2(tArm, ll) * 0.5);
                float bracket = min(s1, s2);
                // 裁掉第一象限之外的部分，否则 abs 会把臂镜像到卡心
                bracket = max(bracket, max(lp.x, lp.y));
                bracket = 1.0 - smoothstep(-aa, aa, bracket);

                // 45° 斜刻
                //
                // 两条约束，缺一条都会被一眼看出来：
                //   ① 四个角必须是**同一个形状转 90° 的整数倍**（把左下角顺时针转 90°
                //      摆到左上角的位置，要跟左上角一模一样）。所以方向不能用屏幕常量。
                //   ② 它是**横跨角平分线**的一道斜杠（垂直于"指向卡心"的方向），
                //      不是顺着平分线指进卡心。这两种只差 90°，整块气质完全不同 ——
                //      顺着指是"箭头"，横着切是"切口"，美术要的是后者。
                //
                // 踩过的三个坑（都记在这，别再往回改）：
                //   · abs(p) 镜像：镜像不是旋转 → 倾角两两相反。
                //   · 旋转矩阵 + 盒子偏移"翻回来"：盒子中心落负半边的假设只在 k=+1 成立，
                //     另两个角的图形整根跑到另一条对角线，被裁成 1/3 长。
                //   · 屏幕固定方向 (1,-1)：不跟着角转，只有左上角恰好对，
                //     左下角那根一半扎进竖臂（看起来像"从竖臂上探出的一小段"）。
                //
                // 现在：中心 = 角点沿两条边各内退 _SpurOffset（用 sign(p) 还原四角，不用 abs），
                //       方向 = 把"指向卡心"的平分线方向 (-sign) 再转 90°。
                float2 spSign = float2(p.x < 0.0 ? -1.0 : 1.0, p.y < 0.0 ? -1.0 : 1.0);
                float2 spurC = spSign * (halfCard - _SpurOffset * bs);
                // (-sign) 指向卡心，(sign.y, -sign.x) 是它转 +90° → 横跨平分线的斜杠
                float2 spurDir = float2(spSign.y, -spSign.x) * 0.70710678;
                float2 sq = p - spurC;
                float2 sqr = float2(dot(sq, spurDir), dot(sq, float2(-spurDir.y, spurDir.x)));
                float sLen = _SpurLength * bs;
                float spurSD = BoxSDF(sqr, float2(0.0, 0.0),
                                      float2(sLen, _SpurWidth * bs) * 0.5);
                float spur = (1.0 - smoothstep(-aa, aa, spurSD)) * 0.85;

                // ---- 合成 ----
                // 最外那条细线**完全不参与时间变化**（它是"外层"，定住不动）。
                float3 colOuter = _OuterColor.rgb * lineOuter;

                // 其余全部属于"内层"：主金带 / 白热芯 / 角标 / 斜刻 / 内侧辉光，
                // 统一乘同一条呼吸曲线。辉光也放进来 —— 它面积最大，是最容易被"看见"的部分；
                // 只让几条细线呼吸的话，整块画面看起来像静止的。
                float3 colInner = _MainColor.rgb    * lineMain
                                + _CoreColor.rgb    * lineCore
                                + _MainColor.rgb    * halo
                                + _BracketColor.rgb * bracket * (1.0 + _BracketPress * press)
                                + _BracketColor.rgb * spur;

                float3 col = (colOuter + colInner * breathe) * _Intensity;

                // 手工写 rgb/a，不要用 fixed4(col, a) —— fixed 只有 11 位精度，
                // 加色用的 HDR 亮度（>1）会被截断，亮部出现色阶。
                //
                // 预乘输出：rgb = 加上去的光，a = 遮盖背景的程度。
                //
                // 关键：**只有实线参与遮盖，辉光不参与**。
                // 辉光如果也带 a，浅色卡面上会被 over 混成一片橘雾（整张卡变桃子色）；
                // 让它保持 a = 0，就等于回到纯加色 —— 白底上顶成白、看不见，卡面保持干净，
                // 这正是我们想要的（浅色底上本来就无法用"更亮"来表达高亮）。
                float3 add = col * IN.color.rgb * IN.color.a;

                float solid = max(lineSolid, max(bracket, spur));
                float cover = saturate(solid + rim) * IN.color.a;

                float rimAmt = rim * IN.color.a;
                add += _RimColor.rgb * rimAmt;          // 深棕直接压上去，黑底上看不出

                float4 color;
                color.rgb = add;
                color.a = cover;

                #ifdef UNITY_UI_CLIP_RECT
                float clip01 = UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                color.rgb *= clip01;
                color.a *= clip01;
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(color.a - 0.001);
                #endif

                return color;
            }
            ENDCG
        }
    }

    Fallback "UI/Default"
}
