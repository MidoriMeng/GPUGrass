﻿Shader "GPUGrass/Grass" {
    Properties{

        _MainTex("Albedo (RGB)", 2D) = "white" {}
        _AlphaTex("Alpha (A)", 2D) = "white" {}
        _Height("Grass Height", float) = 3
        _Width("Grass Width", range(0, 0.1)) = 0.05

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
            #pragma geometry geom
            #pragma multi_compile_instancing
            #include "UnityLightingCommon.cginc"

            #pragma target 4.0

            sampler2D _MainTex;
            sampler2D _AlphaTex;

            float _Height;//草的高度
            float _Width;//草的宽度

            uint localIndex = -1;

            #define MAX_PATCH_SIZE 1024
            #define TILE_SIZE 1

            float4 _patchRootsPosDir[MAX_PATCH_SIZE];//TODO
            float _patchGrassHeight[MAX_PATCH_SIZE];
            float _patchDensities[MAX_PATCH_SIZE];

            struct v2g
            {
                float4 pos : SV_POSITION;
                float3 norm : NORMAL;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID// necessary only if you want to access instanced properties in geometry Shader.
            };

            struct g2f
            {
                float4 pos : SV_POSITION;
                float3 norm : NORMAL;
                float2 uv : TEXCOORD0;
            };


            static const float oscillateDelta = 0.05;

            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float4, _tileWorldCoordStartIndex)
                UNITY_DEFINE_INSTANCED_PROP(float4, _tileHeightDelta)
            UNITY_INSTANCING_BUFFER_END(Props)

            v2g vert(appdata_full v)
            {
                localIndex = (localIndex + 1) % 64;
                float4 tileWorldCoordIndex = UNITY_ACCESS_INSTANCED_PROP(Props, _tileWorldCoordStartIndex);
                float3 density = _patchDensities[localIndex + tileWorldCoordIndex.w];
                /*if (density < mapDensity)
                    discard;*/

                v2g o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);//necessary only if you want to access instanced properties in geometry Shader.
                
                o.pos = v.vertex;
                o.norm = v.normal;
                o.uv = v.texcoord;


                return o;
            }

            g2f createGSOut() {
                g2f output;

                output.pos = float4(0, 0, 0, 0);
                output.norm = float3(0, 0, 0);
                output.uv = float2(0, 0);

                return output;
            }

            float getY(float x2, float y2, float z2, float x3, float y3, float z3, float x4, float z4) {
                float A = y2 * z3 / z2 / y3,
                      B = x2 * z3 / z2 / x3 + 0.000001,
                      C = x2 * y3 / y2 / x3;
                return (-C * z4 - A * x4) / B;
            }

            [maxvertexcount(30)]
            void geom(point v2g points[1], inout TriangleStream<g2f> triStream) {

                float4 tileWorldCoordIndex = UNITY_ACCESS_INSTANCED_PROP(Props, _tileWorldCoordStartIndex);
                float3 heightDelta = UNITY_ACCESS_INSTANCED_PROP(Props, _tileHeightDelta).xyz;
                float3 grassRoot = _patchRootsPosDir[localIndex + tileWorldCoordIndex.w].xyz;
                //float direction = 
                
                float4 root = points[0].pos;//点的位置-根

                //求deltaY
                //A(0,0,0)   C(TILE_SIZE, heightDelta.y, TILE_SIZE) 
                //B(TILE_SIZE, heightDelta.x, 0)   D(0, heightDelta.z, TILE_SIZE)
                /*float x3, y3, z3;
                if(grassRoot.z>grassRoot.x){//ACD
                    x3 = 0; y3 = heightDelta.z; z3 = TILE_SIZE;
                }
                else {//ACB
                    x3 = TILE_SIZE; y3 = heightDelta.x; z3 = 0;
                }

                float3 root = float4(grassRoot, 0) +
                    float4(0, getY(TILE_SIZE, heightDelta.y, TILE_SIZE, x3, y3, z3, grassRoot.x, grassRoot.z), 0, 0);
                */

                const int vertexCount = 12;

                float random = sin(UNITY_HALF_PI * frac(root.x) + UNITY_HALF_PI * frac(root.z));


                _Width = _Width + (random / 50);
                _Height = _Height + (random / 5);


                g2f v[vertexCount] = {
                    createGSOut(), createGSOut(), createGSOut(), createGSOut(),
                    createGSOut(), createGSOut(), createGSOut(), createGSOut(),
                    createGSOut(), createGSOut(), createGSOut(), createGSOut()
                };

                //处理纹理坐标
                float currentV = 0;
                float offsetV = 1.f / ((vertexCount / 2) - 1);

                //处理当前的高度
                float currentHeightOffset = 0;
                float currentVertexHeight = 0;

                //风的影响系数
                float windCoEff = 0;

                for (int i = 0; i < vertexCount; i++)
                {
                    v[i].norm = float3(0, 0, 1);

                    if (fmod(i , 2) == 0)
                    {
                        v[i].pos = float4(root.x - _Width , root.y + currentVertexHeight, root.z, 1);
                        v[i].uv = float2(0, currentV);
                    }
                    else
                    {
                        v[i].pos = float4(root.x + _Width , root.y + currentVertexHeight, root.z, 1);
                        v[i].uv = float2(1, currentV);

                        currentV += offsetV;
                        currentVertexHeight = currentV * _Height;
                    }

                    //
                    float2 wind = float2(sin(_Time.x * UNITY_PI * 5), sin(_Time.x * UNITY_PI * 5));
                    wind.x += (sin(_Time.x + root.x / 25) + sin((_Time.x + root.x / 15) + 50)) * 0.5;
                    wind.y += cos(_Time.x + root.z / 80);
                    wind *= lerp(0.7, 1.0, 1.0 - random);

                    float oscillationStrength = 2.5f;
                    float sinSkewCoeff = random;
                    float lerpCoeff = (sin(oscillationStrength * _Time.x + sinSkewCoeff) + 1.0) / 2;
                    float2 leftWindBound = wind * (1.0 - oscillateDelta);
                    float2 rightWindBound = wind * (1.0 + oscillateDelta);

                    wind = lerp(leftWindBound, rightWindBound, lerpCoeff);

                    float randomAngle = lerp(-UNITY_PI, UNITY_PI, random);
                    float randomMagnitude = lerp(0, 1., random);
                    float2 randomWindDir = float2(sin(randomAngle), cos(randomAngle));
                    wind += randomWindDir * randomMagnitude;

                    float windForce = length(wind);

                    v[i].pos.xz += wind.xy * windCoEff;
                    v[i].pos.y -= windForce * windCoEff * 0.8;
                    //


                    v[i].pos = UnityObjectToClipPos(v[i].pos);

                    if (fmod(i, 2) == 1) {

                        windCoEff += offsetV;
                    }

                }

                for (int p = 0; p < (vertexCount - 2); p++) {
                    triStream.Append(v[p]);
                    triStream.Append(v[p + 2]);
                    triStream.Append(v[p + 1]);
                }
            }


            half4 frag(g2f IN) : COLOR
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


#if defined(SHADER_API_PSSL)
                return float4(1, 0, 0, 1);
#endif
                return float4(color.rgb * light, alpha.g);
            }

            ENDCG

        }
    }
}