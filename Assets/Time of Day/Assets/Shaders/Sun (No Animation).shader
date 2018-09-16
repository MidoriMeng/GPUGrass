// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Time of Day/Sun (No Animation)"
{
	Properties
	{
		_MainTex ("Alpha (A)", 2D) = "white" {}
	}

	SubShader
	{
		Tags
		{
			"Queue"="Transparent-480"
			"RenderType"="Transparent"
			"IgnoreProjector"="True"
		}

		Pass
		{
			Cull Back
			ZWrite Off
			ZTest LEqual
			Blend One One
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

			struct v2f {
				float4 position : SV_POSITION;
				float3 tex      : TEXCOORD0;
			};

			v2f vert(appdata_base v) {
				v2f o;

				o.position = TOD_TRANSFORM_VERT(v.vertex);
				o.tex.xy   = v.texcoord;
				o.tex.z    = (mul(TOD_World2Sky, mul(unity_ObjectToWorld, v.vertex)).y + TOD_Horizon) * 25;

				return o;
			}

			half4 frag(v2f i) : COLOR {
				half4 color = half4(_Color, 1);

				half alpha = tex2D(_MainTex, i.tex.xy).a * saturate(i.tex.z);
				alpha = alpha * _Brightness;
				alpha = pow(alpha, _Contrast);

				color.rgb *= alpha;

				return color;
			}

			ENDCG
		}
	}

	Fallback Off
}
