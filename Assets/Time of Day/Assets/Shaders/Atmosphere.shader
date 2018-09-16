Shader "Time of Day/Atmosphere"
{
	Properties
	{
		_DitheringTexture ("Dithering Lookup Texture (A)", 2D) = "black" {}
	}

	SubShader
	{
		Tags
		{
			"Queue"="Transparent-460"
			"RenderType"="Transparent"
			"IgnoreProjector"="True"
		}

		Pass
		{
			Cull Front
			ZWrite Off
			ZTest LEqual
			Blend One One
			Fog { Mode Off }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			#include "TOD_Base.cginc"
			#include "TOD_Scattering.cginc"

			#define BAYER_DIM 8.0

			uniform sampler2D _DitheringTexture;

			struct v2f {
				float4 position : SV_POSITION;
				half4  color    : TEXCOORD0;
				half2  frag     : TEXCOORD1;
			};

			v2f vert(appdata_base v) {
				v2f o;

				o.color = ScatteringColor(v.normal);
				o.position = TOD_TRANSFORM_VERT(v.vertex);

				float4 projPos = ComputeScreenPos(o.position);
				o.frag = projPos.xy / projPos.w * _ScreenParams.xy * (1.0 / BAYER_DIM);

				return o;
			}

			half4 frag(v2f i) : COLOR {
				half dither = tex2D(_DitheringTexture, i.frag).a * (1.0 / (BAYER_DIM * BAYER_DIM + 1.0));
				return i.color + dither;
			}

			ENDCG
		}
	}

	Fallback Off
}
