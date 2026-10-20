Shader "Custom/CellSquareShader"
{
    Properties
    {
        _BgColor("Background Color", Color) = (0.35,0.28,0.20,1)
        _BorderColor("Border Color", Color) = (0.77,0.36,0.24,1)
        _ShowBorder("Show Border", Range(0,1)) = 0
        _BorderThickness("Border Thickness", Range(0,0.5)) = 0.5
        _GradientRange("Gradient Range", Range(0,0.2)) = 0.2
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            float4 _BgColor;
            float4 _BorderColor;
            float _ShowBorder;
            float _BorderThickness;
            float _GradientRange;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;
                // 到四边最近距离
                float edgeDist = min( min(uv.x, 1-uv.x), min(uv.y,1-uv.y) );

                fixed4 col = _BgColor;
                if(_ShowBorder > 0.5)
                {
                    // 向外边缘做渐变：edge越小越靠近边框
                    float borderMask = smoothstep(_BorderThickness, _BorderThickness + _GradientRange, edgeDist);
                    float alpha = 1-borderMask;
                    col = lerp(col, _BorderColor, alpha);
                }
                return col;
            }
            ENDCG
        }
    }
}
