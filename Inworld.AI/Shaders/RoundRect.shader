Shader "Inworld/UI/RoundRect"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _BoarderColor("BoarderColor", Color) = (1,1,1,1)
        _Radius("Radius", Range(0,0.5)) = 0
        _Boarder("Boarder", Range(0, 0.5)) = 0
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
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
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                float2 adaptUV: TETCOORD2;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            fixed _Radius;
            fixed _Boarder;
            fixed4 _Color;
            fixed4 _BoarderColor;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                // YAN: Set the UV Range from (0-1, 0-1) to (-0.5 ~ 0.5, -0.5 ~ 0.5)
                OUT.adaptUV = OUT.texcoord - fixed2(0.5, 0.5);
                OUT.color = v.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                half4       color;
                const half4 regular_color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;

                if (abs(IN.adaptUV).x <  0.5 - _Radius 
                    || abs(IN.adaptUV).y < 0.5 - _Radius
                    || length(abs(IN.adaptUV) - fixed2(0.5 - _Radius, 0.5 - _Radius)) < _Radius)
                {                    
                    color = _BoarderColor;
                }
                else
                {
                    discard;
                }
                // 2. Get Inner RoundRect, paint with real color.
                if (abs(IN.adaptUV).x <  0.5 - _Radius && abs(IN.adaptUV).y < 0.5 - _Radius)
                {                   
                    // YAN: Remain the central square.
                    color = regular_color; 
                }
                else if ((abs(IN.adaptUV).x < 0.5 - _Radius && abs(IN.adaptUV).y < 0.5 - _Boarder)
                    || (abs(IN.adaptUV).y < 0.5 - _Radius && abs(IN.adaptUV).x < 0.5 - _Boarder))
                {
                    // YAN: Remain the 4 rect outside the square
                    color = regular_color; 
                }                    
                else
                {
                    if (length(abs(IN.adaptUV) - fixed2(0.5 - _Radius, 0.5 - _Radius)) < _Radius - _Boarder)
                        color = regular_color; 
                }
                
                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip (color.a - 0.001);
                #endif

                return color;
            }
        ENDCG
        }
    }
}