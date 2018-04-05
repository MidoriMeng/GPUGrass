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
            Tags{
            "LightMode" = "ForwardBase"
            }
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
            #pragma target 4.0
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"
            #include "UnityInstancing.cginc"

			struct appdata
			{
                UNITY_VERTEX_INPUT_INSTANCE_ID
				float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
                UNITY_VERTEX_INPUT_INSTANCE_ID
                float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
                float3 normal : TEXCOORD1;
			};

            UNITY_INSTANCING_BUFFER_START(prop)
                UNITY_DEFINE_INSTANCED_PROP(float4, _tileHeightDeltaStartIndex)
            UNITY_INSTANCING_BUFFER_END(prop)

			sampler2D _MainTex;
			float4 _MainTex_ST;
			
			v2f vert (appdata v)
            {
                v2f o;
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                o.pos = mul(UNITY_MATRIX_VP, mul(unity_ObjectToWorld, v.vertex));

                //o.pos = UnityObjectToClipPosInstanced(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.normal = UnityObjectToWorldNormal(v.normal);
                return o;
            }
			
			fixed4 frag (v2f i) : SV_Target
			{
                UNITY_SETUP_INSTANCE_ID(i);
				fixed4 col = tex2D(_MainTex, i.uv);
                return col;
			}
			ENDCG
		}
	}
}
