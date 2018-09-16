Shader "Time of Day/Skybox"
{
	Properties
	{
	}

	SubShader
	{
		Tags
		{
			"Queue"="Background"
			"RenderType"="Background"
			"PreviewType"="Skybox"
		}

		Pass
		{
			Cull Off
			ZWrite Off
			ZTest LEqual

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			#include "TOD_Base.cginc"
			#include "TOD_Scattering.cginc"

			struct v2f {
				float4 position : SV_POSITION;
				half4  color    : TEXCOORD0;
			};

			v2f vert(appdata_base v) {
				v2f o;

				o.color = (v.vertex.y < 0) ? half4(pow(TOD_AmbientColor, TOD_Contrast), 1) : ScatteringColor(v.vertex);
				o.position = TOD_TRANSFORM_VERT(v.vertex);

				return o;
			}

			half4 frag(v2f i) : COLOR {
				return i.color;
			}

			ENDCG
		}
	}

	Fallback Off
}
