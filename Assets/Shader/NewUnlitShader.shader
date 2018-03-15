﻿// Upgrade NOTE: upgraded instancing buffer 'InstanceProperties' to new syntax.

Shader "test/NewUnlitShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
        _tileHeightDeltaStartIndex("height delta index", Vector) = (0,0,0,0)
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
            #pragma multi_compile_instancing
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
                UNITY_VERTEX_INPUT_INSTANCE_ID
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

            UNITY_INSTANCING_BUFFER_START(prop)
                UNITY_DEFINE_INSTANCED_PROP(float4, _tileHeightDeltaStartIndex)
            UNITY_INSTANCING_BUFFER_END(prop)

			sampler2D _MainTex;
			float4 _MainTex_ST;
			
			v2f vert (appdata v)
			{
				v2f o;
                UNITY_SETUP_INSTANCE_ID(v);//
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
				return col;
			}
			ENDCG
		}
	}
}
