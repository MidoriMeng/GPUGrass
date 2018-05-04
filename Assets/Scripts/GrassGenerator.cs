using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class GrassGenerator : MonoBehaviour {
    public Texture2D grassDensityMap;
    public const int PATCH_SIZE = 2;//Patch的边长
    public int grassAmountPerTile = 64;//实际渲染时每个Tile内最多的草叶数量
    public int pregenerateGrassAmount = 1023;//预生成Patch草体总长度
    private int bladeSectionCount = 5;//草叶分段，5段12顶点，6段14顶点
    public Material grassMaterial;
    //public float patchExpansion = 2f;//实际渲染时比视锥体范围扩展的距离，为了保证边界渲染的质量且减少视锥体裁剪的频率
    

    /// <summary>
    /// 预生成草地信息数组，传输给grassMaterial
    /// </summary>
    public void PregenerateGrassInfo() {
        Vector3Int startPosition = Vector3Int.zero;
        System.Random random = new System.Random();
        //grassRoots = new List<Vector3>();
        List<Vector4> grassRootsDir = new List<Vector4>();//草的位置+随机方向，[0,1]
        List<float> grassHeights = new List<float>();
        List<float> grassDensityIndexes = new List<float>();

        //随机生成草根位置、方向、高度、密度索引
        for (int i = 0; i < pregenerateGrassAmount; i++) {
            float deltaX = (float)random.NextDouble();
            float deltaZ = (float)random.NextDouble();
            Vector3 root = new Vector3(deltaX * PATCH_SIZE, 0, deltaZ * PATCH_SIZE);

            //grassRoots.Add(root);
            grassRootsDir.Add(new Vector4(root.x, root.y, root.z, (float)random.NextDouble()));
            grassHeights.Add(0.5f + 0.5f * (float)random.NextDouble());
            grassDensityIndexes.Add((float)random.NextDouble());
        }

        //send to gpu
        grassMaterial.SetInt("_SectionCount", bladeSectionCount);
        Shader.SetGlobalFloat("_TileSize", PATCH_SIZE);
        grassMaterial.SetVectorArray(Shader.PropertyToID("_patchRootsPosDir"), grassRootsDir);
        grassMaterial.SetFloatArray(Shader.PropertyToID("_patchGrassHeight"), grassHeights);
        grassMaterial.SetFloatArray(Shader.PropertyToID("_patchDensities"), grassDensityIndexes);
    }


    /// <summary>
    /// generate a mesh with assigned numbers of points
    /// </summary>
    public Mesh generateGrassTile() {
        Mesh result = new Mesh();
        int bladeVertexCount = (bladeSectionCount + 1) * 2;
        Vector3[] normals = new Vector3[grassAmountPerTile * bladeVertexCount];
        Vector3[] vertices = new Vector3[grassAmountPerTile * bladeVertexCount];
        Vector2[] uv = new Vector2[grassAmountPerTile * bladeVertexCount];
        for (int i = 0; i < vertices.Length; i++) {
            //赋予x坐标，为了使其作为索引在gpu中读取数组信息
            vertices[i] = new Vector3(i / bladeVertexCount, i % bladeVertexCount, 0);//0-63,0-11,0
            normals[i] = -Vector3.forward;
            uv[i] = new Vector2(i % bladeVertexCount % 2,
                ((float)(i % bladeVertexCount / 2)) / bladeSectionCount);
        }
        result.vertices = vertices;

        int[] triangles = new int[6 * grassAmountPerTile * bladeSectionCount];
        int trii = 0;
        for (int blade = 0; blade < grassAmountPerTile; blade++) {
            for (int section = 0; section < bladeSectionCount; section++) {
                int start = blade * bladeVertexCount + section * 2;
                triangles[trii] = start;
                triangles[trii + 1] = start + 3;
                triangles[trii + 2] = start + 1;

                triangles[trii + 3] = start;
                triangles[trii + 4] = start + 2;
                triangles[trii + 5] = start + 3;
                trii += 6;
            }
        }
        result.triangles = triangles;
        result.normals = normals;
        result.uv = uv;
        return result;
    }

    
    ///show grass
        /*GameObject grass = new GameObject("grass", typeof(MeshRenderer), typeof(MeshFilter));
        grass.transform.parent = transform;
        grass.GetComponent<MeshFilter>().mesh = grassMesh;
        grass.GetComponent<MeshRenderer>().sharedMaterial = grassMaterial;
        */
}
