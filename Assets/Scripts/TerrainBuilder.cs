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


    /*public struct TileBuffer {
        public Vector4 worldCoordinateStartIndex;//xyz:world coordinate, w:start index
        public Vector4 heightDelta;
    };*/

    /// <summary>
    /// 根据指定的高度图生成地面网格，将地表信息存储到tilebuffer
    /// </summary>
    public void BuildTerrain() {
        //重新生成地形前需要清除之前的信息
        MeshFilter f = GetComponent<MeshFilter>();
        if (f)
            DestroyImmediate(f);
        MeshRenderer r = GetComponent<MeshRenderer>();
        if (r)
            DestroyImmediate(r);

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

        MeshFilter filter = gameObject.AddComponent<MeshFilter>();
        MeshRenderer renderer = gameObject.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = terrainMat;

        Mesh terrainMesh = new Mesh();
        terrainMesh.vertices = vertices.ToArray();
        terrainMesh.uv = uvs;
        terrainMesh.triangles = triangles.ToArray();
        //normals
        terrainMesh.RecalculateNormals();
        filter.mesh = terrainMesh;
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


    /*public void RaiseGrass() {
        if (grassLayer)
            DestroyImmediate(grassLayer.gameObject);
        Vector3Int startPosition = Vector3Int.zero;//待定
        Vector3Int curPos = startPosition;
        System.Random random = new System.Random();
        grassRoots = new List<Vector3>();
        directions = new List<float>();
        grassHeights = new List<float>();
        grassDensityIndexs = new List<float>();

        //随机生成草根位置、方向、高度、密度索引
        int density = grassAmountPerPatch / patchSize / patchSize;
        for (int i = 0; i < patchSize; i++) {
            for (int j = 0; j < patchSize; j++) {
                for (int k = 0; k < density; k++) {
                    float deltaX = (float)random.NextDouble();
                    float deltaZ = (float)random.NextDouble();
                    float nextX = heightMap.GetPixel(curPos.x + 1, curPos.z).grayscale * terrainHeight;
                    float nextZ = heightMap.GetPixel(curPos.x, curPos.z + 1).grayscale * terrainHeight;
                    float curHeight = heightMap.GetPixel(curPos.x, curPos.z).grayscale * terrainHeight;

                    grassRoots.Add(new Vector3(curPos.x + deltaX,
                        curHeight + deltaX * (nextX - curHeight) + deltaZ * (nextZ - curHeight),
                        curPos.z + deltaZ));
                    directions.Add((float)random.NextDouble());
                    grassHeights.Add(grassHeightMax * 0.5f + grassHeightMax * 0.5f * (float)random.NextDouble());
                    grassDensityIndexs.Add((float)random.NextDouble());
                }
                curPos += new Vector3Int(0, 0, 1);
            }
            curPos += new Vector3Int(1, 0, -4);
        }

        //生成草地模型
        Mesh m = new Mesh();
        m.vertices = grassRoots.ToArray();
        int[] indices = new int[grassRoots.Count];
        for (int i = 0; i < indices.Length; i++)
            indices[i] = i;
        m.SetIndices(indices, MeshTopology.Points, 0);
        grassLayer = (new GameObject("GrassLayer")).transform;
        grassLayer.transform.parent = gameObject.transform;
        MeshFilter filter = grassLayer.gameObject.AddComponent<MeshFilter>();
        MeshRenderer renderer = grassLayer.gameObject.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = grassMaterial;
        filter.mesh = m;
    }*/

}
