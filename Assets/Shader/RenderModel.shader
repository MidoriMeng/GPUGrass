      Shader "Instanced/InstancedShader" {
    Properties {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
    }
    SubShader {

        Pass {

            Tags {"LightMode"="ForwardBase"}

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
            #pragma target 4.5

            #include "UnityCG.cginc"
            #include "Data.cginc"
            #include "Lighting.cginc"

        #if SHADER_TARGET >= 45
            StructuredBuffer<float3> renderPosAppend;
        #endif

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 uv : TEXCOORD0;
                uint instanceID : SV_InstanceID;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : TEXCOORD1;
                float3 test: TEXCOORD2;
            };


            struct GrassData {
                float height, density;
                float4 rootDir;
            };

            sampler2D _MainTex;
            sampler2D _AlphaTex;
            float4 _MainTex_ST;

            float _Height;//草的高度
            float _Width;//草的宽度
            int _SectionCount;//草叶的分段数

            static const float oscillateDelta = 0.05;
            static const float PI = 3.14159;


            float4 _patchRootsPosDir[MAX_PATCH_SIZE];//TODO
            float _patchGrassHeight[MAX_PATCH_SIZE];
            float _patchDensities[MAX_PATCH_SIZE];
            StructuredBuffer<GrassData> _patchData;
            // GrassData _patchData[5];
            int pregenerateGrassAmount;
            int grassAmountPerTile;

            v2f vert (appdata v, uint instanceID : SV_InstanceID)
            {
                v2f o;
                UNITY_INITIALIZE_OUTPUT(v2f, o);
            #if SHADER_TARGET >= 45
                float3 index = renderPosAppend[instanceID];
            #else
                float3 index = 0;
            #endif

                float3 localPosition = v.vertex.xyz;
                float3 worldPosition = index*2 + localPosition;
                float3 worldNormal = v.normal;


                o.pos = mul(UNITY_MATRIX_VP, float4(worldPosition, 1.0f));
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 color = tex2D(_MainTex, i.uv);
                fixed4 alpha = tex2D(_AlphaTex, i.uv);
                half3 worldNormal = UnityObjectToWorldNormal(i.normal);
                //ads
                fixed3 light;

                //ambient
                fixed3 ambient = ShadeSH9(half4(worldNormal, 1));

                //diffuse
                fixed3 diffuseLight = saturate(dot(worldNormal, UnityWorldSpaceLightDir(i.pos))) * _LightColor0;

                //specular Blinn-Phong 
                fixed3 halfVector = normalize(UnityWorldSpaceLightDir(i.pos) + WorldSpaceViewDir(i.pos));
                fixed3 specularLight = pow(saturate(dot(worldNormal, halfVector)), 15) * _LightColor0;

                light = ambient + diffuseLight + specularLight;
                return float4(color.rgb * light, alpha.g);
            }

            ENDCG
        }
    }
}