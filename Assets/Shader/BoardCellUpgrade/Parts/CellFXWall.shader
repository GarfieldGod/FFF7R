Shader "FFF7/VFX/CellFX/Wall"
{
    // 部件 3 · 光墙。顺着卡牌轮廓（含四角圆弧）立起来的一圈竖直面。
    //
    // 进度映射全在这一个文件里：
    //   0 → _RiseEnd          从下往上涨满（head 0 → 1.18）
    //   _RiseEnd → _DissolveStart   停在满高（这段就是"墙立着"）
    //   _DissolveStart → 1    **消散**（不是落下去）
    //
    // 消散有两种口味，_DissolveSweep 在中间插值：
    //   0 = 整体淡出      （墙一起变淡）
    //   1 = 从下往上抽走  （底边先化开，顶边最后没）
    //
    // 水平方向完全均匀：frag 不读 uv.x，所以四面和圆弧天生一致，不会读成"一根根光柱"。
    // 纯加色（a = 0）。
    Properties
    {
        // 材质上只留「整套皮肤共享」的：配色与质感曲线。
        // 涨落节奏（_RiseEnd / _DissolveStart / ...）由组件（CellFXWall）用 MPB 写入，下面隐藏。
        [Header(Appearance)]
        _DeepColor      ("墙 · 底部深色", Color) = (0.10, 0.45, 0.85, 1)
        _PaleColor      ("墙 · 顶部浅色", Color) = (0.70, 0.95, 1.00, 1)
        _HotColor       ("墙根白热",      Color) = (0.94, 0.99, 1.00, 1)
        _WallGain       ("墙亮度",        Range(0, 3))     = 1.05
        _WallFalloff    ("由实变虚",      Range(0.2, 6))   = 1.55
        _TintPow        ("由深变浅的快慢", Range(0.3, 4))  = 1.15
        _BaseHot        ("墙根白热亮度",  Range(0, 2))     = 0.14
        _BaseSoft       ("贴地淡入高度",  Range(0, 0.4))   = 0.12
        _GrowSoft       ("顶端软边",      Range(0.01, 0.6)) = 0.22

        [Header(Common)]
        _Tint           ("色调",           Color) = (1, 1, 1, 1)
        _Intensity      ("整体亮度",       Range(0.05, 4))  = 1.00
        _Level          ("等级 0-3",       Range(0, 3))     = 1
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest ("深度测试（3D 用 4）", Float) = 4

        // ---- 以下由 CellFXWall.cs 通过 MPB 写入，材质上不显示 ----
        [HideInInspector] _RiseEnd      ("涨满时的进度",  Range(0.02, 0.9)) = 0.18
        [HideInInspector] _HeadScale    ("升满时的高出量", Range(0.8, 1.4)) = 1.18
        [HideInInspector] _DissolveStart("开始消散的进度", Range(0.1, 0.95)) = 0.55
        [HideInInspector] _DissolveSoft ("消散的软边",    Range(0.02, 0.6)) = 0.28
        [HideInInspector] _DissolveSweep("消散方式",      Range(0, 1)) = 1.0
        [HideInInspector] _Progress     ("进度",          Range(0, 1))     = 0
        [HideInInspector] _Unit         ("卡牌短边",      Float) = 0.32
    }

    SubShader
    {
        Tags { "Queue"="Transparent+10" "RenderType"="Transparent" "IgnoreProjector"="True" }
        // 不能改成 Cull Back：远侧墙法线背离相机，会被整片剔掉，而那正是画面里最主要的面积。
        // 代价是同一层被正反面各算一遍，亮度 ×2。
        Cull Off
        ZWrite Off
        ZTest [_ZTest]
        Blend One One          // 纯加色

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

            float4 _DeepColor, _PaleColor, _HotColor, _Tint;
            float _WallGain, _WallFalloff, _TintPow, _BaseHot, _BaseSoft, _GrowSoft, _HeadScale;
            float _RiseEnd, _DissolveStart, _DissolveSoft, _DissolveSweep;
            float _Progress, _Unit, _Intensity, _Level;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv  = v.uv;
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float vy = saturate(i.uv.y);                       // 0 = 贴地，1 = 顶端

                // ---- 涨：顶端用 _GrowSoft 做软边，所以是"光往上漫"，不是整体淡入 ----
                float up     = smoothstep(0.0, max(_RiseEnd, 1e-3), _Progress);
                float head   = up * _HeadScale;
                float reveal = saturate((head - vy) / max(_GrowSoft, 1e-3));

                // ---- 散：_DissolveStart → 1 ----
                float dd    = saturate((_Progress - _DissolveStart) / max(1e-3, 1.0 - _DissolveStart));
                // 消散前沿从"地面以下"一路扫到顶上：dd=0 时前沿在 -2×soft（整面都在前面，全亮），
                // dd=1 时前沿到 1（整面都在后面，全没）。软边就是这个前沿的厚度。
                float soft  = max(_DissolveSoft, 1e-3);
                float front = dd * (1.0 + 2.0 * soft) - 2.0 * soft;
                float sweep = smoothstep(front, front + soft, vy);

                // 整体淡出那一档同时乘着，保证进度走到 1 时一定归零
                float keep  = (1.0 - dd) * lerp(1.0, sweep, _DissolveSweep);

                float bsoft = smoothstep(0.0, max(_BaseSoft, 1e-3), vy);   // 贴地淡入
                float dens  = pow(1.0 - vy, _WallFalloff);                  // 由实变虚
                float3 grad = lerp(_DeepColor.rgb, _PaleColor.rgb,
                                   pow(vy, max(_TintPow, 1e-3)));          // 由深变浅

                float lv = saturate(_Level / 3.0);
                float3 col = grad * (dens * _WallGain * bsoft);
                col += _HotColor.rgb * (_BaseHot * exp(-vy * 6.0) * bsoft);

                col *= _Intensity * reveal * keep * (1.0 + 0.30 * lv);
                col *= _Tint.rgb;

                return float4(col, 0.0);                   // a = 0，纯加色
            }
            ENDCG
        }
    }

    Fallback Off
}
