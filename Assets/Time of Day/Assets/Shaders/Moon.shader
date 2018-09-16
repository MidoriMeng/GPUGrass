// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Time of Day/Moon"
{
	Properties
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}

	SubShader
	{
		Tags
		{
			"Queue"="Transparent-470"
			"RenderType"="Transparent"
			"IgnoreProjector"="True"
		}

		Pass
		{
			Cull Back
			ZWrite Off
			ZTest LEqual
			Fog { Mode Off }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			#include "TOD_Base.cginc"

			uniform float3 _Color;
			uniform float _Contrast;
			uniform float _Brightness;

			uniform sampler2D _MainTex;
			uniform float4 _MainTex_ST;

			struct v2f {
				float4 position : SV_POSITION;
				float3 tex      : TEXCOORD0;
				float3 normal   : TEXCOORD1;
			};

			v2f vert(appdata_base v) {
				v2f o;

				float3 viewdir = normalize(ObjSpaceViewDir(v.vertex));

				float3 skyPos = mul(TOD_World2Sky, mul(unity_ObjectToWorld, v.vertex)).xyz;

				o.position = TOD_TRANSFORM_VERT(v.vertex);
				o.tex.xy   = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.tex.z    = (skyPos.y + TOD_Horizon) * 25;
				o.normal   = normalize(mul((float3x3)unity_ObjectToWorld, v.normal));

				return o;
			}

			half4 frag(v2f i) : COLOR {
				half4 color = half4(_Color, 1);

				half alpha = max(0, dot(i.normal, -TOD_SunDirection));
				alpha = alpha * saturate(i.tex.z) * _Brightness;
				alpha = pow(alpha, _Contrast);

				half3 moontex = tex2D(_MainTex, i.tex).rgb;

				color.rgb *= moontex * alpha;

				return color;
			}

			ENDCG
		}
	}

	Fallback Off
}
