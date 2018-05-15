      Shader "Instanced/renderGrass" {
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


            float rand(float3 co)
            {
                return frac(sin(dot(co.xyz, float3(12.9898, 78.233, 45.5432))) * 43758.5453);
            }

            float3 setupHDI(float3 index, out int patchIndex) {
                float3 hdi;
                hdi.x = getTerrainPos(float2(index.x + 1, index.z));
                hdi.y = getTerrainPos(float2(index.x + 1, index.z + 1));
                hdi.z = getTerrainPos(float2(index.x, index.z + 1));
                float random = rand(index);
                patchIndex = (int)(random * (pregenerateGrassAmount - grassAmountPerTile));
                return hdi;
            }

            //形成草叶形状
            float3 getBladeOffset(float3 index, float3 vertex,
                float uvv, uint patchIndex) {
                //变量准备
                uint bladeIndex = vertex.x + patchIndex;//0~63+0~1023-64
                uint vertIndex = vertex.y;//0~11
                GrassData patchInfo = _patchData[bladeIndex];
                float density = patchInfo.density;
                float4 rootLPos = patchInfo.rootDir.xyzz;

                float dir = patchInfo.rootDir.w * 2 * PI,
                    height = patchInfo.height * _Height;

                //计算
                //return 1 or -1          //
                float4 bladeOffset = float4((fmod(vertIndex, 2) * 2 - 1) * _Width, uvv *height, 0, 0);

                //风
                float3 windVec = float3(1, 0, 0);
                //blade bending
                float bending = fmod(bladeIndex, 3)*0.5 + 0.2;
                float a = -height / (bending * bending), b = 2 * height / bending;
                float deltaZ = (-b + sqrt(b*b + 4 * a*(uvv * height))) / (2 * a);
                bladeOffset.z += deltaZ;
                //blade swinging

                float sin, cos;
                sincos(dir, /*out*/ sin, /*out*/ cos);
                bladeOffset = float4(bladeOffset.x*cos + bladeOffset.z*sin,
                    bladeOffset.y,
                    -bladeOffset.x*sin + bladeOffset.z*cos, 0);
                return bladeOffset;
            }

            float3 getLocalRootPos(float3 index, float3 vertex, out uint patchIndex) {
                float3 hd = setupHDI(index, patchIndex);//height delta
                //uint vertexCount = (_SectionCount + 1) * 2;//12
                uint bladeIndex = vertex.x + patchIndex;//0~63+0~1023-64
                GrassData patchInfo = _patchData[bladeIndex];
                float4 rootLPos = patchInfo.rootDir.xyzz; 
                rootLPos.w = 0;//local pos in tile

                               //计算deltaY：本地Y增量（高低）
                               //A(0,0,0)   C(_TileSize, hd.y, _TileSize) 
                               //B(_TileSize, hd.x, 0)   D(0, hd.z, _TileSize)
                float x3, y3, z3, deltaY;
                if (rootLPos.z + rootLPos.x <= _TileSize) {//ABD
                                                           //y=(yb*x+yd*z)/l
                    deltaY = (hd.x * rootLPos.x + hd.z * rootLPos.z) / _TileSize;
                }
                else {//CBD
                      //C(0, 0, 0)  B(0, hd.x - hd.y, -_TileSize)  D(-_TileSize, hd.z - hd.y, 0)
                      //(rootLPos.x - _TileSize, ?, rootLPos.z - _TileSize)
                      //y'=-(yd'*x'+yb'*z')/l
                    deltaY = -((hd.z - hd.y)*(rootLPos.x - _TileSize) + (hd.x - hd.y) * (rootLPos.z - _TileSize))
                        / _TileSize + hd.y;
                }
                return rootLPos + float3(0, deltaY, 0);
            }

            v2f vert (appdata v, uint instanceID : SV_InstanceID)
            {
                v2f o;
                UNITY_INITIALIZE_OUTPUT(v2f, o);
            #if SHADER_TARGET >= 45
                float3 index = renderPosAppend[instanceID];
            #else
                float3 index = 0;
            #endif
                //float4 hdi = setupHDI(index);
                uint patchIndex;
                float4 worldStartPos = getTerrainPos(index.xz);//按理说应该有y
                float3 localPosition = getLocalRootPos(index, v.vertex.xyz, patchIndex);
                localPosition += getBladeOffset(index, v.vertex.xyz, v.uv.y, patchIndex);
                float3 worldPosition = worldStartPos + localPosition;
                float3 worldNormal = v.normal;


                o.pos = mul(UNITY_MATRIX_VP, float4(worldPosition, 1.0f));
                o.test = terrainHeightTex[index.xz];
                return o;
            }



            fixed4 frag (v2f i) : SV_Target
            {
                return float4(i.test,1);
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