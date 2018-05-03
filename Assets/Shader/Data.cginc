#define MAX_PATCH_SIZE 1023
float _TileSize = 2.0;//需要计算
float4 threadSize;//视锥体贴图大小，frustumTexSizeX, frustumTexSizeY in c#
float4 terrainSize;//场景大小（index）

struct TerrainData {
    float height;
    float hasGrass;
    float grassDensity;
    //wind
};

RWStructuredBuffer<TerrainData> terrainDataBuffer;
float4 _FrustumStartPosI;//每帧更新，视锥体块的起点
RWStructuredBuffer<uint> indirectDataBuffer;

int flattenId(int2 index, int2 mapSize) {
    return index.x + index.y * mapSize.x;
}

float4 getTerrainPos(int2 id) {
    int flatid = flattenId(id, terrainSize.xy);
    return float4(id.x*_TileSize, terrainDataBuffer[flatid].height, id.y*_TileSize, 0);
    
}