using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainBuilder : MonoBehaviour {
    //terrain
    public Texture2D heightMap;
    public float terrainHeight = 5f;
    private float terrainScale = 1f;
    public Material terrainMat;

    /// <summary>
    /// 根据指定的高度图生成地面网格，将地表信息存储到tilebuffer
    /// </summary>
    public void BuildTerrain() {
        //重新生成地形前需要清除之前的信息
        MeshFilter filter = GetComponent<MeshFilter>();
        if (!filter)
            filter = gameObject.AddComponent<MeshFilter>();
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (!renderer)
            renderer = gameObject.AddComponent<MeshRenderer>();
        MeshCollider collider = GetComponent<MeshCollider>();
        if (!collider)
            collider = gameObject.AddComponent<MeshCollider>();

        //生成地形
        List<Vector3>  vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        for (int i = 0; i < heightMap.width; i++) {
            for (int j = 0; j < heightMap.height; j++) {
                //vertices
                vertices.Add(new Vector3(
                    i * terrainScale,
                    heightMap.GetPixel(i, j).grayscale * terrainHeight,
                    j * terrainScale));
                if (i == 0 || j == 0)
                    continue;
                //triangles
                triangles.Add(heightMap.width * i + j);
                triangles.Add(heightMap.width * i + j - 1);
                triangles.Add(heightMap.width * (i - 1) + j - 1);
                triangles.Add(heightMap.width * (i - 1) + j - 1);
                triangles.Add(heightMap.width * (i - 1) + j);
                triangles.Add(heightMap.width * i + j);
            }
        }
        Vector2[] uvs = new Vector2[vertices.Count];
        for (int i = 0; i < uvs.Length; i++) {
            //uvs
            uvs[i] = new Vector2(vertices[i].x * terrainScale,
                vertices[i].z * terrainScale);
        }

        renderer.sharedMaterial = terrainMat;

        Mesh terrainMesh = new Mesh();
        terrainMesh.vertices = vertices.ToArray();
        terrainMesh.uv = uvs;
        terrainMesh.triangles = triangles.ToArray();
        //normals
        terrainMesh.RecalculateNormals();
        filter.mesh = terrainMesh;
        collider.sharedMesh = terrainMesh;
    }

    public void BuildTerrainDataBuffer() {
        int bufLen = heightMap.width * heightMap.height;
        ComputeBuffer buffer = new ComputeBuffer(bufLen, new TerrainData().size());
        TerrainData[] data = new TerrainData[bufLen];
        for (int i = 0; i < heightMap.height; i++) {
            for (int j = 0; j < heightMap.width; j++) {
                data[i * heightMap.width + j] =
                    new TerrainData(heightMap.GetPixel(j, i).grayscale * terrainHeight);
            }
        }
        buffer.SetData(data);
        Shader.SetGlobalBuffer("terrainDataBuffer", buffer);
        Shader.SetGlobalVector("terrainSize", new Vector4(heightMap.width, heightMap.height));
    }


    public Vector2Int GetConstrainedTileIndex(int indexX, int indexZ) {
        indexX = Mathf.Max(0, indexX); indexX = Mathf.Min(indexX, heightMap.width / GrassGenerator.PATCH_SIZE - 2);
        indexZ = Mathf.Max(0, indexZ); indexZ = Mathf.Min(indexZ, heightMap.height / GrassGenerator.PATCH_SIZE - 2);
        return new Vector2Int(indexX, indexZ);
    }
    public Vector2Int GetConstrainedTileIndex(Vector2Int index) {
        return GetConstrainedTileIndex(index.x, index.y);
    }

    public Vector2Int GetConstrainedTileIndex(Vector3 position) {
        return GetConstrainedTileIndex(GetTileIndex(position));
    }

    /// <summary>
    /// 从vertices读取位置
    /// </summary>
    public Vector3 GetTilePosition(int i, int j) {
        Vector2Int index = GetConstrainedTileIndex(i, j);
        float height= heightMap.GetPixel(index.x, index.y).grayscale * terrainHeight;
        return new Vector3(index.x * terrainScale, height, index.y * terrainScale);
        //return vertices[index.x * GrassGenerator.PATCH_SIZE * heightMap.width + index.y * GrassGenerator.PATCH_SIZE];
    }

    public Vector3 GetTilePosition(Vector2Int index) {
        return GetTilePosition(index.x, index.y);
    }

    public Vector2Int GetTileIndex(Vector3 position) {
        return new Vector2Int(Mathf.FloorToInt(position.x / GrassGenerator.PATCH_SIZE),
            Mathf.FloorToInt(position.z / GrassGenerator.PATCH_SIZE));
    }
    

    struct TerrainData {
        float height;
        float hasGrass;//整数是否显示草，小数草密度
        float grassDensity;
        //wind
        public TerrainData(float height) {
            this.height = height;
            hasGrass = 0;
            grassDensity = 0;
        }
        public int size() { return sizeof(float) * 3; }
    };


}
