using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainBuilder {
    private Texture2D heightMap;

    public float terrainHeight = 5f;
    public const int PATCH_SIZE = 2;//Patch的边长
    private Material terrainMat;

    private float terrainScale = 1f;

    /// <summary>
    /// 根据指定的高度图生成地面网格，将地表信息存储到tilebuffer
    /// </summary>
    public GameObject BuildTerrain(Transform parent) {
        //重新生成地形前需要清除之前的信息
        GameObject obj = new GameObject("terrain", typeof(MeshFilter),typeof(MeshRenderer),typeof(MeshCollider));
        obj.transform.parent = parent;
        MeshFilter filter = obj.GetComponent<MeshFilter>();
        MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
        MeshCollider collider = obj.GetComponent<MeshCollider>();

        //生成地形
        List<Vector3>  vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        for (int i = 0; i < heightMap.width; i++) {
            for (int j = 0; j < heightMap.height; j++) {
                //vertices
                vertices.Add(new Vector3(
                    i * terrainScale,
                    heightMap.GetPixel(i, j).r * terrainHeight,
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
        return obj;
    }

    /*public ComputeBuffer BuildTerrainDataBuffer() {
        int bufLen = heightMap.width * heightMap.height;
        terrainDataBuffer = new ComputeBuffer(bufLen, new TerrainData().size());
        TerrainData[] data = new TerrainData[bufLen];
        for (int i = 0; i < heightMap.height; i++) {
            for (int j = 0; j < heightMap.width; j++) {
                float height = heightMap.GetPixel(j, i).grayscale * terrainHeight;
                data[i * heightMap.width + j] =
                    new TerrainData(height);
            }
        }
        terrainDataBuffer.SetData(data);
        return terrainDataBuffer;
    }*/

    public Texture GetTerrainHeightTexture() {
        /*RenderTexture res = new RenderTexture(
            heightMap.width, heightMap.height, 24);
        Graphics.Blit(heightMap, res);*/
        return heightMap;
    }


    public Vector2Int GetConstrainedTileIndex(int indexX, int indexZ) {
        indexX = Mathf.Clamp(indexX, 0, heightMap.width / PATCH_SIZE - 2);
        indexZ = Mathf.Clamp(indexZ, 0, heightMap.height / PATCH_SIZE - 2);
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
        float height= heightMap.GetPixel(index.x, index.y).r * terrainHeight;
        return new Vector3(index.x * terrainScale, height, index.y * terrainScale);
        //return vertices[index.x * GrassGenerator.PATCH_SIZE * heightMap.width + index.y * GrassGenerator.PATCH_SIZE];
    }

    public Vector3 GetTilePosition(Vector2Int index) {
        return GetTilePosition(index.x, index.y);
    }

    public Vector2Int GetTileIndex(Vector3 position) {
        return new Vector2Int(Mathf.FloorToInt(position.x / PATCH_SIZE),
            Mathf.FloorToInt(position.z / PATCH_SIZE));
    }

    /*public void ReleaseBuffer() {
        terrainDataBuffer.Release();
    }*/

    /*struct TerrainData {
        float height;
        float hasGrass;//整数是否显示草，小数草密度
        float grassDensity;
        //wind
        public TerrainData(float height) {
            this.height = height;
            hasGrass = 0;
            grassDensity = 0.5f;
        }
        public int size() { return sizeof(float) * 3; }
    };*/

    public TerrainBuilder(Texture2D heightMap, float terrainHeight,Material terrainMat) {
        this.heightMap = heightMap;
        this.terrainHeight = terrainHeight;
        this.terrainMat = terrainMat;
    }
    
}
