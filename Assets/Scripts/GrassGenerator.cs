using UnityEngine;

public class GrassGenerator : MonoBehaviour {
    public Texture2D grassDensityMap;
    public int grassAmountPerTile = 64;//实际渲染时每个Tile内最多的草叶数量
    public int pregenerateGrassAmount = 1023;//预生成Patch草体总长度
    private int bladeSectionCount = 5;//草叶分段，5段12顶点，6段14顶点
    public Material grassMaterial;
    //public float patchExpansion = 2f;//实际渲染时比视锥体范围扩展的距离，为了保证边界渲染的质量且减少视锥体裁剪的频率
    
    struct GrassData {
        public float height, density;
        public Vector4 rootDir;
        public GrassData(float height, float density, Vector4 rootDir) {
            this.height = height;this.density = density;this.rootDir = rootDir;
        }
    };

    /// <summary>
    /// 预生成草地信息数组，传输给grassMaterial
    /// </summary>
    public void PregenerateGrassInfo() {
        Vector3Int startPosition = Vector3Int.zero;
        System.Random random = new System.Random();
        GrassData[] grassData = new GrassData[pregenerateGrassAmount];
        int PATCH_SIZE = TerrainBuilder.PATCH_SIZE;
        //随机生成草根位置、方向、高度、密度索引
        for (int i = 0; i < pregenerateGrassAmount; i++) {
            float deltaX = (float)random.NextDouble();
            float deltaZ = (float)random.NextDouble();
            Vector3 root = new Vector3(deltaX * PATCH_SIZE, 0, deltaZ * PATCH_SIZE);

            GrassData data = new GrassData(0.5f + 0.5f * (float)random.NextDouble(),
                (float)random.NextDouble(),
                new Vector4(root.x, root.y, root.z, (float)random.NextDouble()));
            grassData[i] = data;
        }
        ComputeBuffer grassBuffer = new ComputeBuffer(pregenerateGrassAmount, sizeof(float) * 6);
        grassBuffer.SetData(grassData);
        //send to gpu
        grassMaterial.SetInt("_SectionCount", bladeSectionCount);
        Shader.SetGlobalFloat("_TileSize", PATCH_SIZE);
        grassMaterial.SetBuffer("_patchData", grassBuffer);
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
