Shader "BillboardShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
	    [Enum(RenderOnTop, 0,RenderWithTest, 4)] _ZTest("Render on top", Int) = 1
        _Color ("Tint", Color) = (1,1,1,1)

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
        ZWrite Off
        ZTest [_ZTest]
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
                UNITY_VERTEX_OUTPUT_STEREO
            };

            fixed4 _Color;
            fixed4 _TextureSampleAdd;
	        float4 _FaceObject;
	        
	        inline float4x4 CalcRotationMatrix() {
	            float3 vertNormal = float3(0, 0, 1);
                float3 centerWorldPos = mul(unity_ObjectToWorld, float4(0.0, 0.0, 0.0, 1.0)).xyz;
                float3 faceModelDir = normalize(_FaceObject.xyz - centerWorldPos);
                float dotProd = dot(vertNormal, faceModelDir);
                float3 crossProd = cross(vertNormal, faceModelDir);
                float4x4 vMat = {
                    0, crossProd.z, -crossProd.y, 0,
                    -crossProd.z, 0, crossProd.x, 0,
                    crossProd.y, -crossProd.x, 0, 0,
                    0, 0, 0, 1
                };
                float4x4 unitMat = {
                    1, 0, 0, 0,
                    0, 1, 0, 0,
                    0, 0, 1, 0,
                    0, 0, 0, 1
                };
                return unitMat + vMat + mul(mul(vMat, vMat), 1 / (1 + dotProd));
	        }

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                float4x4 rotMat = CalcRotationMatrix();
		        OUT.vertex = UnityObjectToClipPos(mul(v.vertex, rotMat));
                OUT.texcoord = v.texcoord;
                OUT.color = v.color * _Color;
                return OUT;
            }

            sampler2D _MainTex;

            fixed4 frag(v2f IN) : SV_Target
            {
                return tex2D(_MainTex, IN.texcoord) * IN.color;
            }
        ENDCG
        }
    }
}