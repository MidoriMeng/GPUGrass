Shader "GPUGrass/Grass" {
    Properties{

        _MainTex("Albedo (RGB)", 2D) = "white" {}
        _AlphaTex("Alpha (A)", 2D) = "white" {}
        _Height("Grass Height", float) = 3
        _Width("Grass Width", range(0, 0.1)) = 0.05
        _SectionCount("section count", int) = 5
            _TileSize("section count", float) = 2.0

    }

    SubShader{
        Cull off
        Tags{ "Queue" = "AlphaTest" "RenderType" = "TransparentCutout" "IgnoreProjector" = "True" }


        Pass {

            Cull OFF
            Tags{ "LightMode" = "ForwardBase" }
            AlphaToMask On


            CGPROGRAM

            #include "UnityCG.cginc" 
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "UnityLightingCommon.cginc"

            #pragma target 4.0

            sampler2D _MainTex;
            sampler2D _AlphaTex;

            float _Height;//草的高度
            float _Width;//草的宽度
            int _SectionCount;//草叶的分段数
            float _TileSize;

            #define MAX_PATCH_SIZE 1023
            #define _TileSize 2

            float4 _patchRootsPosDir[MAX_PATCH_SIZE];//TODO
            float _patchGrassHeight[MAX_PATCH_SIZE];
            float _patchDensities[MAX_PATCH_SIZE];

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 norm : NORMAL;
                float2 uv : TEXCOORD0;
            };

            static const float oscillateDelta = 0.05;

            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float4, _tileHeightDeltaStartIndex)
            UNITY_INSTANCING_BUFFER_END(Props)

            float getY(float x2, float y2, float z2, float x3, float y3, float z3, float x4, float z4) {
                float A = y2 * z3 / z2 / y3,
                    B = x2 * z3 / z2 / x3 + 0.000001,
                    C = x2 * y3 / y2 / x3;
                return (-C * z4 - A * x4) / B;
            }

            v2f vert(appdata_full v)
            {
                v2f o;
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_SETUP_INSTANCE_ID(v);
                float4 heightDeltaIndex = UNITY_ACCESS_INSTANCED_PROP(Props, _tileHeightDeltaStartIndex);
                int bladeIndex = v.vertex.x + heightDeltaIndex.w;//0~63
                int vertIndex = v.vertex.y;//0~11
                float3 density = _patchDensities[bladeIndex];
                /*if (density < mapDensity)
                    discard;*/
                o.norm = float3(0, 0, 1);
                o.uv = v.texcoord;
                //计算o.pos
                float3 root = _patchRootsPosDir[bladeIndex].xyz;//local pos
                //确定y
                //A(0,0,0)   C(_TileSize, heightDeltaIndex.y, _TileSize) 
                //B(_TileSize, heightDeltaIndex.x, 0)   D(0, heightDeltaIndex.z, _TileSize)
                float x3, y3, z3;
                if(root.z > root.x){//ACD
                    x3 = 0; y3 = heightDeltaIndex.z; z3 = _TileSize;
                }
                else {//ACB
                    x3 = _TileSize; y3 = heightDeltaIndex.x; z3 = 0;
                }
                root +=
                float3(0, getY(_TileSize, heightDeltaIndex.y, _TileSize, x3, y3, z3, o.pos.x, o.pos.z), 0);
                //确定o.pos
                float dir = _patchRootsPosDir[bladeIndex].w, height = _patchGrassHeight[bladeIndex];
                uint vertexCount = (_SectionCount + 1) * 2;
                //处理纹理坐标
                float currentV = 0;
                float offsetV = 1.f / ((vertexCount / 2) - 1);
                //处理当前的高度
                float currentHeightOffset = 0;
                float currentVertexHeight = 0;
                if (fmod(vertIndex, 2) == 0)
                {
                    o.pos = float4(root.x - _Width, root.y + currentVertexHeight, root.z, 1);
                    o.uv = float2(0, currentV);
                }
                else
                {
                    o.pos = float4(root.x + _Width, root.y + currentVertexHeight, root.z, 1);
                    o.uv = float2(1, currentV);

                    currentV += offsetV;
                    currentVertexHeight = currentV * _Height;
                }
                //v[vertIndex].pos = UnityObjectToClipPos(v[i].pos);
                o.pos = mul(UNITY_MATRIX_VP, mul(unity_ObjectToWorld, o.pos));
                return o;
            }

            half4 frag(v2f IN) : COLOR
            {
                fixed4 color = tex2D(_MainTex, IN.uv);
                fixed4 alpha = tex2D(_AlphaTex, IN.uv);

                half3 worldNormal = UnityObjectToWorldNormal(IN.norm);

                //ads
                fixed3 light;

                //ambient
                fixed3 ambient = ShadeSH9(half4(worldNormal, 1));

                //diffuse
                fixed3 diffuseLight = saturate(dot(worldNormal, UnityWorldSpaceLightDir(IN.pos))) * _LightColor0;

                //specular Blinn-Phong 
                fixed3 halfVector = normalize(UnityWorldSpaceLightDir(IN.pos) + WorldSpaceViewDir(IN.pos));
                fixed3 specularLight = pow(saturate(dot(worldNormal, halfVector)), 15) * _LightColor0;

                light = ambient + diffuseLight + specularLight;

                return float4(color.rgb * light, alpha.g);
            }

            ENDCG

        }
    }
}