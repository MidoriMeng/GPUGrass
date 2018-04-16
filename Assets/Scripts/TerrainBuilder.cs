using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainBuilder : MonoBehaviour {
    //terrain
    public Texture2D heightMap;
    public float terrainHeight = 5f;
    public float terrainScale = 1f;
    public Material terrainMat;
    [HideInInspector]
    public List<Vector3> vertices;

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
        vertices = new List<Vector3>();
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
    
    public Vector2Int GetConstrainedTileIndex(int indexX, int indexZ) {
        indexX = Mathf.Max(0, indexX); indexX = Mathf.Min(indexX, heightMap.width / GrassBuilder.PATCH_SIZE - 2);
        indexZ = Mathf.Max(0, indexZ); indexZ = Mathf.Min(indexZ, heightMap.height / GrassBuilder.PATCH_SIZE - 2);
        return new Vector2Int(indexX, indexZ);
    }
    
    /// <summary>
    /// 从vertices读取位置
    /// </summary>
    public Vector3 GetTilePosition(int i, int j) {
        //Debug.Log(i + "  " + j+" " + vertices.Count);
        Vector2Int index = GetConstrainedTileIndex(i, j);
        return vertices[index.x * GrassBuilder.PATCH_SIZE * heightMap.width + index.y * GrassBuilder.PATCH_SIZE];
    }

    public Vector3 GetTilePosition(Vector2Int index) {
        return GetTilePosition(index.x, index.y);
    }

    public Vector2Int GetTileIndex(Vector3 position) {
        return new Vector2Int(Mathf.FloorToInt(position.x / GrassBuilder.PATCH_SIZE),
            Mathf.FloorToInt(position.z / GrassBuilder.PATCH_SIZE));
    }

    private void Awake() {
        BuildTerrain();
    }
    
}
