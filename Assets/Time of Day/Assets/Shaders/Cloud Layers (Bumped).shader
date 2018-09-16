Shader "Time of Day/Cloud Layers (Bumped)"
{
	Properties
	{
		_NoiseTexture1 ("Noise Texture 1 (A)", 2D) = "white" {}
		_NoiseTexture2 ("Noise Texture 2 (A)", 2D) = "white" {}
		_NoiseNormals1 ("Noise Normals 1",     2D) = "bump"  {}
		_NoiseNormals2 ("Noise Normals 2",     2D) = "bump"  {}
	}

	SubShader
	{
		Tags
		{
			"Queue"="Transparent-450"
			"RenderType"="Transparent"
			"IgnoreProjector"="True"
		}

		Pass
		{
			Cull Front
			ZWrite Off
			ZTest LEqual
			Blend SrcAlpha OneMinusSrcAlpha
			Fog { Mode Off }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			#include "TOD_Base.cginc"

			uniform float _SunGlow;
			uniform float _MoonGlow;
			uniform float _CloudDensity;
			uniform float _CloudSharpness;
			uniform float2 _CloudScale1;
			uniform float2 _CloudScale2;
			uniform float4 _CloudUV;

			uniform sampler2D _NoiseTexture1;
			uniform sampler2D _NoiseTexture2;
			uniform sampler2D _NoiseNormals1;
			uniform sampler2D _NoiseNormals2;

			struct v2f {
				float4 position : SV_POSITION;
				fixed4 color    : TEXCOORD0;
				float4 cloudUV  : TEXCOORD1;
				float3 lightdir : TEXCOORD2;
			};

			v2f vert(appdata_tan v) {
				v2f o;

				// Vertex position and uv coordinates
				float3 vertnorm = normalize(v.vertex.xyz);
				float2 vertuv   = vertnorm.xz / pow(vertnorm.y + 0.1, 0.75);
				float  vertfade = saturate(100 * vertnorm.y * vertnorm.y);

				// Light directions
				float3 lightvec = -TOD_LocalLightDirection;
				float3 sunvec   = -TOD_LocalSunDirection;
				float3 moonvec  = -TOD_LocalMoonDirection;

				// Glow factors
				float sunglow  = _SunGlow  * max(0, dot(v.normal, sunvec));
				float moonglow = _MoonGlow * max(0, dot(v.normal, moonvec));

				// Cloud color
				float3 cloudcol = TOD_CloudColor + sunglow * TOD_SunColor + moonglow * TOD_MoonColor;

				// Write results
				o.position   = TOD_TRANSFORM_VERT(v.vertex);
				o.color.rgb  = cloudcol;
				o.color.a    = _CloudDensity * vertfade;
				o.cloudUV.xy = (vertuv + _CloudUV.xy) / _CloudScale1;
				o.cloudUV.zw = (vertuv + _CloudUV.zw) / _CloudScale2;
				o.lightdir   = lightvec;

				// Adjust output color according to gamma value
				// Doing this in the vertex shader is approximate but faster than doing it in the fragment shader
				o.color.rgb = pow(o.color.rgb, TOD_OneOverGamma);

				return o;
			}

			fixed4 frag(v2f i) : COLOR {
				fixed4 color = i.color;

				// Sample texture
				fixed noise1 = tex2D(_NoiseTexture1, i.cloudUV.xy).a;
				fixed noise2 = tex2D(_NoiseTexture2, i.cloudUV.zw).a;
				fixed a = i.color.a * pow(noise1 * noise2, _CloudSharpness);

				// Apply texture
				color.a = saturate(a);

				// Sample normals
				fixed4 noiseNormal1 = tex2D(_NoiseNormals1, i.cloudUV.xy);
				fixed4 noiseNormal2 = tex2D(_NoiseNormals2, i.cloudUV.zw);
				fixed3 cloud_normal = UnpackNormal(0.5 * (noiseNormal1 + noiseNormal2));
				fixed cloud_shading = pow(0.1 + (1 + dot(cloud_normal, i.lightdir)) / 2, TOD_OneOverGamma);

				// Apply normal map based shading
				color.rgb *= cloud_shading;

				return color;
			}

			ENDCG
		}
	}

	Fallback Off
}
