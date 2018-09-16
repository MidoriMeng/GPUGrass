// Upgrade NOTE: replaced '_Projector' with 'unity_Projector'

Shader "Time of Day/Cloud Shadows (2)"
{
	Properties
	{
		_NoiseTexture1 ("Noise Texture 1 (A)", 2D) = "white" {}
		_NoiseTexture2 ("Noise Texture 2 (A)", 2D) = "white" {}
	}

	SubShader
	{
		Pass
		{
			Offset -1, -1
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			#include "TOD_Base.cginc"

			uniform float _Alpha;
			uniform float _CloudDensity;
			uniform float _CloudSharpness;
			uniform float2 _CloudScale1;
			uniform float2 _CloudScale2;
			uniform float4 _CloudUV;

			uniform sampler2D _NoiseTexture1;
			uniform sampler2D _NoiseTexture2;
			uniform float4x4 unity_Projector;

			struct v2f {
				float4 position : SV_POSITION;
				float4 cloudUV  : TEXCOORD0;
			};

			v2f vert(appdata_base v)
			{
				v2f o;

				float3 vertnorm = -TOD_LocalLightDirection;
				float2 vertuv   = vertnorm.xz / pow(vertnorm.y + 0.1, 0.75);

				float4 projPos  = mul(unity_Projector, v.vertex);
				float2 uvoffset = 0.5 + projPos.xy / projPos.w;

				o.position   = TOD_TRANSFORM_VERT(v.vertex);
				o.cloudUV.xy = uvoffset + (vertuv + _CloudUV.xy) / _CloudScale1;
				o.cloudUV.zw = uvoffset + (vertuv + _CloudUV.zw) / _CloudScale2;

				return o;
			}

			fixed4 frag(v2f i) : COLOR
			{
				fixed noise1 = tex2D(_NoiseTexture1, i.cloudUV.xy).a;
				fixed noise2 = tex2D(_NoiseTexture2, i.cloudUV.zw).a;
				fixed a = _CloudDensity * pow(noise1 * noise2, _CloudSharpness);

				return fixed4(0, 0, 0, saturate(a) * _Alpha);
			}

			ENDCG
		}
	}

	Fallback Off
}
