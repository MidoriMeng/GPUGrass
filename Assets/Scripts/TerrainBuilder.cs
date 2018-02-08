using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainBuilder : MonoBehaviour {
    //terrain
    public Texture2D heightMap;
    public float terrainHeight = 5f;
    public int terrainSizeX = 128;
    public int terrainSizeZ = 128;
    public Material terrainMat;
    //grass
    public Texture2D grassDensityMap;
    public float grassHeight = 2f;
    public int patchSize = 4;
    public int grassAmountPerPatch = 64;
    public Material grassMaterial;
    private List<Vector3> positions;
    private List<Vector3> directions;
    private List<float> lengths;
    private List<float> densityIndexs;
    private List<Vector3> roots;
    private Transform grassLayer;

    public void BuildTerrain() {
        //重新生成地形前需要清除之前的地形
        MeshFilter f = GetComponent<MeshFilter>();
        if (f)
            DestroyImmediate(f);
        MeshRenderer r = GetComponent<MeshRenderer>();
        if (r)
            DestroyImmediate(r);

        //生成地形
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        for (int i = 0; i < heightMap.width; i++) {
            for (int j = 0; j < heightMap.height; j++) {
                //vertices
                vertices.Add(new Vector3(
                    i * heightMap.width / terrainSizeX,
                    heightMap.GetPixel(i, j).grayscale * terrainHeight,
                    j * heightMap.height / terrainSizeZ));
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
            uvs[i] = new Vector2(vertices[i].x / heightMap.width * terrainSizeX,
                vertices[i].z / heightMap.height * terrainSizeZ);
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

    public void RaiseGrass() {
        if (grassLayer)
            DestroyImmediate(grassLayer.gameObject);
        Vector3 startPosition = Vector3.zero;//待定
        //高度待定
        Vector3 curPos = startPosition;
        System.Random random = new System.Random();
        roots = new List<Vector3>();
        //随机生成位置
        int density = grassAmountPerPatch / patchSize / patchSize;
        for (int i = 0; i < patchSize; i++) {

            for (int j = 0; j < patchSize; j++) {
                for (int k = 0; k < density; k++) {
                    roots.Add(new Vector3((float)(curPos.x + random.NextDouble()),
                        0, (float)(curPos.z + random.NextDouble())));
                }
                curPos += new Vector3(0, 0, 1);
            }
            curPos += new Vector3(1, 0, -4);
        }

        //生成草地
        Mesh m = new Mesh();
        m.vertices = roots.ToArray();
        int[] indices = new int[grassAmountPerPatch];
        for (int i = 0; i < indices.Length; i++)
            indices[i] = i;
        m.SetIndices(indices, MeshTopology.Points, 0);
        grassLayer = (new GameObject("GrassLayer")).transform;
        grassLayer.transform.parent = gameObject.transform;
        MeshFilter filter = grassLayer.gameObject.AddComponent<MeshFilter>();
        MeshRenderer renderer = grassLayer.gameObject.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = grassMaterial;
        filter.mesh = m;
    }
}
