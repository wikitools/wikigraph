Shader "LZWPlib_Examples/TextureAnimationExample"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_TintColor("Tint Color", Color) = (0,0,0,1)
		_AnimationSpeed("Animation speed", vector) = (-5, -5, 0, 0)
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			#include "Assets/LZWPlib/Shaders/LZWPlibCG.cginc"  // include declarations: _LzwpTime, _LzwpSinTime, _LzwpCosTime

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

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _TintColor;
			float4 _AnimationSpeed;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);

				o.uv += _AnimationSpeed * _LzwpTime.x;   // shift the UVs over time

				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv) + _TintColor;
				return col;
			}
			ENDCG
		}
	}
}
