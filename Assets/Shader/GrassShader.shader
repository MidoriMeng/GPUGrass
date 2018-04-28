Shader "GPUGrass/GrassShader"
{
	Properties
	{
		_MainTex("Albedo (RGB)", 2D) = "white" {}
        _AlphaTex("Alpha (A)", 2D) = "white" {}
        _Height("Grass Height", float) = 3
        _Width("Grass Width", range(0, 0.1)) = 0.05
        _SectionCount("section count", int) = 5
        _TileSize("section count", float) = 2.0
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
            AlphaToMask On
            Cull Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
            #pragma target 4.0
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"
            #include "Lighting.cginc"
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
                float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
                float3 normal : TEXCOORD1;
                float3 test: TEXCOORD2;
			};

            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float4, _tileHeightDeltaStartIndex)
            UNITY_INSTANCING_BUFFER_END(Props)

			sampler2D _MainTex;
            sampler2D _AlphaTex;
			float4 _MainTex_ST;

            float _Height;//草的高度
            float _Width;//草的宽度
            int _SectionCount;//草叶的分段数
            float _TileSize;

            static const float oscillateDelta = 0.05;
            static const float PI = 3.14159;

            #define MAX_PATCH_SIZE 1023
            #define _TileSize 2.0

            float4 _patchRootsPosDir[MAX_PATCH_SIZE];//TODO
            float _patchGrassHeight[MAX_PATCH_SIZE];
            float _patchDensities[MAX_PATCH_SIZE];
			

            float getY(float x2, float y2, float z2, float x3, float y3, float z3, float x4, float z4) {
                float A = y2 * z3 / z2 / y3,
                    B = x2 * z3 / z2 / x3 + 0.000001,
                    C = x2 * y3 / y2 / x3;
                return (-C * z4 - A * x4) / B;
            }

			v2f vert (appdata v)
            {
                v2f o;
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_SETUP_INSTANCE_ID(v);

                //草叶信息
                float4 hdi = UNITY_ACCESS_INSTANCED_PROP(Props, _tileHeightDeltaStartIndex);
                uint vertIndex = v.vertex.y;//0~11
                uint bladeIndex = v.vertex.x + hdi.w;//0~63+0~1023-64
                uint vertexCount = (_SectionCount + 1) * 2;//12
                float3 density = _patchDensities[bladeIndex];
                float dir = _patchRootsPosDir[bladeIndex].w * 2 * PI,
                    height = _patchGrassHeight[bladeIndex];
                float4 rootLPos = _patchRootsPosDir[bladeIndex].xyzz; rootLPos.w = 0;//local pos in tile
                //计算deltaY：本地Y增量（高低）
                //A(0,0,0)   C(_TileSize, hdi.y, _TileSize) 
                //B(_TileSize, hdi.x, 0)   D(0, hdi.z, _TileSize)
                float x3, y3, z3, deltaY;
                if (rootLPos.z + rootLPos.x <= _TileSize) {//ABD
                    //y=(yb*x+yd*z)/l
                    deltaY = (hdi.x * rootLPos.x + hdi.z * rootLPos.z) / _TileSize;
                }
                else {//CBD
                    //C(0, 0, 0)  B(0, hdi.x - hdi.y, -_TileSize)  D(-_TileSize, hdi.z - hdi.y, 0)
                    //(rootLPos.x - _TileSize, ?, rootLPos.z - _TileSize)
                    //y'=-(yd'*x'+yb'*z')/l
                    deltaY = -((hdi.z - hdi.y)*(rootLPos.x - _TileSize) + (hdi.x - hdi.y) * (rootLPos.z - _TileSize))
                        / _TileSize + hdi.y;
                }
                //形成草叶形状
                                             //return 1 or -1          //
                float4 bladeOffset = float4((fmod(vertIndex, 2) * 2 - 1) * _Width, v.uv.y * _Height, 0, 0);
                //blade bending
                float bending = fmod(bladeIndex, 3)*0.5+0.2;
                float a = -_Height / (bending * bending), b = 2 * _Height / bending;
                float deltaZ = (-b + sqrt(b*b + 4 * a*(v.uv.y * _Height))) / (2 * a);
                o.test = deltaZ;
                bladeOffset.z += deltaZ;
                //blade swinging
                float sin, cos;
                sincos(dir, /*out*/ sin, /*out*/ cos);
                bladeOffset = float4(bladeOffset.x*cos + bladeOffset.z*sin,
                    bladeOffset.y,
                    -bladeOffset.x*sin + bladeOffset.z*cos, 0);


                o.pos = rootLPos + float4(0, deltaY, 0, 1) + bladeOffset;
                o.pos = mul(UNITY_MATRIX_VP, mul(unity_ObjectToWorld, o.pos));

                //o.pos = UnityObjectToClipPosInstanced(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.normal = UnityObjectToWorldNormal(v.normal);
                return o;
            }
			
			fixed4 frag (v2f i) : SV_Target
			{
                //return fixed4(i.test.x, i.test.y, i.test.z, 1);
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
