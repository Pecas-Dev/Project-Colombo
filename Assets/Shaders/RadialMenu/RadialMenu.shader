Shader "UI/RadialMenu"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _HighlightColor ("Highlight Color", Color) = (1,1,0,1)
        _DividerColor ("Divider Color", Color) = (0,0,0,1)
        _DividerThickness ("Divider Thickness", Range(0, 10)) = 2
        _HighlightedSection ("Highlighted Section", Int) = -1
        
        [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInspector] _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
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
        ZTest [unity_GUIZTestMode]
        Blend One OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                float2 uv       : TEXCOORD1;
                float2 pixelPos : TEXCOORD2;
            };

            fixed4 _Color;
            fixed4 _HighlightColor;
            fixed4 _DividerColor;
            float _DividerThickness;
            int _HighlightedSection;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(v.vertex);
                OUT.texcoord = v.texcoord;
                OUT.color = v.color * _Color;
                OUT.uv = v.texcoord - 0.5;
                OUT.pixelPos = ComputeScreenPos(OUT.vertex).xy / ComputeScreenPos(OUT.vertex).w;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float angle = atan2(IN.uv.y, IN.uv.x);
                angle = (angle + 3.14159265359) / (2.0 * 3.14159265359); 
                
                angle = frac(angle + 0.75);
                
                int section = (int)(angle * 3);
                
                fixed4 color = _Color;
                
                if (section == _HighlightedSection)
                {
                    color = _HighlightColor;
                }
                
                float distanceFromCenter = length(IN.uv);
                
                float sectionAngle = frac(angle * 3);
                
                float distanceToLine = min(sectionAngle, 1.0 - sectionAngle);
                
                float pixelDistance = distanceToLine * 120.0 * 3.14159265359 / 180.0 * distanceFromCenter * _ScreenParams.y / 2.0;
                
                if (pixelDistance < _DividerThickness)
                {
                    color = _DividerColor;
                }
                
                if (distanceFromCenter > 0.5)
                {
                    color.a = 0;
                }
                
                return color;
            }
            ENDCG
        }
    }
}