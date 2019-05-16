Shader "LZWPlib/ObstacleWallWarning" {
	Properties {
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Base (RGB) Trans (A)", 2D) = "white" {}
	}

	SubShader {
		// https://docs.unity3d.com/Manual/SL-SubShaderTags.html
		Tags{ "Queue" = "Overlay+1000" "RenderType" = "Transparent" "ForceNoShadowCasting" = "True" "IgnoreProjector" = "True" "PreviewType" = "Plane" }

		LOD 200
		ZTest Off
		ZWrite Off

		// https://docs.unity3d.com/Manual/SL-Blend.html
		Blend SrcAlpha OneMinusSrcAlpha // Traditional transparency

		Pass {
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			fixed4 _Color;
			sampler2D _MainTex;
			float4 _MainTex_ST;

			uniform int _Trackers_Length = 0;
			uniform half3 _Trackers[32];

			uniform half _ObstacleWallWarnScale;

			struct appdata {
				float4 vertex : POSITION;
				float4 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				half3 worldPos : TEXCOORD1;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				return o;
			}

			fixed4 frag(v2f inp) : SV_Target
			{
				fixed3 color = tex2D(_MainTex, inp.uv) * _Color;

				half alpha = 0.0;

				for (int i = 0; i < _Trackers_Length; i++)
				{
					alpha += smoothstep(1.0, 0.0, distance(inp.worldPos, _Trackers[i].xyz) / _ObstacleWallWarnScale);
				}

				return fixed4(color, alpha);
			}

			ENDCG
		}
	}
}
