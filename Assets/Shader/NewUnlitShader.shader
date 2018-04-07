Shader "test/NewUnlitShader"
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

                float4 heightDeltaIndex = UNITY_ACCESS_INSTANCED_PROP(Props, _tileHeightDeltaStartIndex);
                int vertIndex = v.vertex.z;//0~11
                v.vertex.z = v.vertex.y / 100 / 16;//for test
                int bladeIndex = v.vertex.y / 100 + heightDeltaIndex.w;//0~63+0~1023-64
                //o.test = heightDeltaIndex.w / (1023-64);
                v.vertex.y %= 100;//为了测试，后期要改回去
                float3 density = _patchDensities[bladeIndex];
                //local pos
                float4 root = _patchRootsPosDir[bladeIndex].xyzz; root.w = 0;
                //deltaY
                //A(0,0,0)   C(_TileSize, heightDeltaIndex.y, _TileSize) 
                //B(_TileSize, heightDeltaIndex.x, 0)   D(0, heightDeltaIndex.z, _TileSize)
                float x3, y3, z3, deltaY;
                if (root.z + root.x < _TileSize) {//ABD
                    deltaY = (heightDeltaIndex.x * root.x + heightDeltaIndex.z * root.z) / _TileSize;
                }
                else {//CBD
                    // deltaY = (x*(2 * yb + yd - yc) + l * (yc - yd - yb) + z * (yc - yb)) / l;
                    deltaY = (root.x*(2 * heightDeltaIndex.x + heightDeltaIndex.z - heightDeltaIndex.y)
                        + _TileSize * (heightDeltaIndex.y - heightDeltaIndex.z - heightDeltaIndex.x)
                        + root.z * (heightDeltaIndex.y - heightDeltaIndex.x)) / _TileSize;
                }
                //bladeOffset
                float dir = _patchRootsPosDir[bladeIndex].w, height = _patchGrassHeight[bladeIndex];
                uint vertexCount = (_SectionCount + 1) * 2;//12
                //处理纹理坐标
                float currentV = 0;
                float offsetV = 1.f / ((vertexCount / 2) - 1);
                /*o.uv= float2(vertIndex % 2,
                    ((float)(vertIndex / 2)) / _SectionCount);*/
                float4 bladeOffset;
                if (fmod(vertIndex, 2) == 0)
                {
                    bladeOffset = float4(-_Width, v.uv.y * _Height, 0, 0);
                }
                else
                {
                    bladeOffset = float4(_Width, v.uv.y * _Height, 0, 0);

                }
                o.pos = root + float4(0, deltaY, 0, 1) + bladeOffset;
                //o.test = mul(unity_ObjectToWorld, o.pos);
                o.pos = mul(UNITY_MATRIX_VP, mul(unity_ObjectToWorld, o.pos));

                //o.pos = UnityObjectToClipPosInstanced(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.normal = UnityObjectToWorldNormal(v.normal);
                return o;
            }
			
			fixed4 frag (v2f i) : SV_Target
			{
                //return fixed4(i.test.x, i.test.y, i.test.z, 1);
				fixed4 col = tex2D(_MainTex, i.uv);
                return col;
			}
			ENDCG
		}
	}
}
