Shader "Time of Day/Clear Alpha"
{
	SubShader
	{
		Tags
		{
			"Queue"="Transparent-455"
			"RenderType"="Transparent"
			"IgnoreProjector"="True"
		}

		Pass
		{
			Cull Front
			ZWrite Off
			ZTest LEqual
			ColorMask A
			Fog { Mode Off }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			#include "TOD_Base.cginc"

			struct v2f {
				float4 position : SV_POSITION;
			};

			v2f vert(appdata_tan v) {
				v2f o;
				o.position = TOD_TRANSFORM_VERT(v.vertex);
				return o;
			}

			fixed4 frag(v2f i) : COLOR {
				return fixed4(0, 0, 0, 0);
			}

			ENDCG
		}
	}

	Fallback Off
}
