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
