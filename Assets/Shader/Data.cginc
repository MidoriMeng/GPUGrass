#define MAX_PATCH_SIZE 1023
float _TileSize = 2.0;//需要计算

struct TerrainData {
    float height;
    float hasGrass;
    float grassDensity;
    //wind
};

//RWStructuredBuffer<TerrainData> terrainDataBuffer;
Texture2D<float> terrainHeightTex;
Texture2D<float> terrainDensityTex;
float terrainHeight;
//float4 _FrustumStartPosI;//每帧更新，视锥体块的起点
RWStructuredBuffer<uint> indirectDataBuffer;
StructuredBuffer<float> sizeBuffer;//场景大小/视锥体贴图大小/视锥体块的起点

int flattenId(int2 index, int2 mapSize) {
    return index.x + index.y * mapSize.x;
}

int2 getTileIndex(int flatid, int2 mapSize) {
    int2 res;
    res.y = flatid / mapSize.x;
    res.x = flatid % mapSize.x;
    return res;
}

float4 getTerrainPos(int2 id) {
    return float4(id.x*_TileSize,
        terrainHeightTex[id*_TileSize] * terrainHeight, id.y*_TileSize, 0);
    /*float2 terrainSize = { sizeBuffer[0],sizeBuffer[1] };
    int flatid = flattenId(id, terrainSize);
    return float4(id.x*_TileSize, terrainDataBuffer[flatid].height, id.y*_TileSize, 0);
    */
}

float getTerrainDensity(int2 id) {
    return terrainDensityTex[id*_TileSize];
}

int2 getPosIndex(float2 pos) {
    return pos / _TileSize;
}

int2 GetConstrainedTileIndex(int2 index) {
    int2 res;
    res.x = clamp(index.x, 0, sizeBuffer[2]-2);
    res.y = clamp(index.y, 0, sizeBuffer[3]-2);
    return res;
}

///theta [0,2PI]
float3 RotateArbitraryLine(float3 v1, float3 v2, float3 pt, float theta)
{
    float3 result = (float3)0;
    float a = v1.x;
    float b = v1.y;
    float c = v1.z;

    float3 p = v2 - v1;
    p = normalize(p);//D3DXVec3Normalize(&p, &p);
    float u = p.x;
    float v = p.y;
    float w = p.z;

    float uu = u * u;
    float uv = u * v;
    float uw = u * w;
    float vv = v * v;
    float vw = v * w;
    float ww = w * w;
    float au = a * u;
    float av = a * v;
    float aw = a * w;
    float bu = b * u;
    float bv = b * v;
    float bw = b * w;
    float cu = c * u;
    float cv = c * v;
    float cw = c * w;

    float sintheta, costheta;
    sincos(theta, sintheta, costheta);

    float4x4 mat;
    mat._m00 = uu + (vv + ww) * costheta;
    mat._m01 = uv * (1 - costheta) + w * sintheta;
    mat._m02 = uw * (1 - costheta) - v * sintheta;
    mat._m03 = 0;

    mat._m10 = uv * (1 - costheta) - w * sintheta;
    mat._m11 = vv + (uu + ww) * costheta;
    mat._m12 = vw * (1 - costheta) + u * sintheta;
    mat._m13 = 0;

    mat._m20 = uw * (1 - costheta) + v * sintheta;
    mat._m21 = vw * (1 - costheta) - u * sintheta;
    mat._m22 = ww + (uu + vv) * costheta;
    mat._m23 = 0;

    mat._m30 = (a * (vv + ww) - u * (bv + cw)) * (1 - costheta) + (bw - cv) * sintheta;
    mat._m31 = (b * (uu + ww) - v * (au + cw)) * (1 - costheta) + (cu - aw) * sintheta;
    mat._m32 = (c * (uu + vv) - w * (au + bv)) * (1 - costheta) + (av - bu) * sintheta;
    mat._m33 = 1;
    return mul(mat, float4(pt,1.0)).xyz;
}