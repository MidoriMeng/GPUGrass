using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class GrassGenerator : MonoBehaviour {
    private Mesh grassMesh;
    private TerrainBuilder tBuilder;
    //grass
    public Texture2D grassDensityMap;
    public float grassHeightMax = 2f;
    public const int PATCH_SIZE = 2;//Patch的长/宽
    public int grassAmountPerTile = 64;//实际渲染时每个Tile内最多的草叶数量
    public int pregenerateGrassAmount = 1023;//预生成Patch草体总长度
    private int bladeSectionCount = 5;//草叶分段，5段12顶点，6段14顶点
    public Material grassMaterial;
    //public float patchExpansion = 2f;//实际渲染时比视锥体范围扩展的距离，为了保证边界渲染的质量且减少视锥体裁剪的频率

    private List<float> grassHeights;
    private List<float> grassDensityIndexes;
    //private List<Vector3> grassRoots;//草的位置
    private List<Vector4> grassRootsDir;//草的位置+随机方向，[0,1]

    //frustum calc
    public ComputeShader frustumCalcShader;
    private int frustumTexSizeX, frustumTexSizeY;
    private int threadGroupSizeX = 2, threadGroupSizeY = 2;
    private int frustumKernel = 0;
    public GameObject plane;//for test
    private RenderTexture frustumTexture;
    ComputeBuffer renderPosAppendBuffer;
    ComputeBuffer counterBuffer;

    //indirect
    private ComputeBuffer argsBuffer;
    private Bounds instanceBound;

    /// <summary>
    /// 预生成草地信息数组，传输给grassMaterial
    /// </summary>
    public void PregenerateGrassInfo() {
        Vector3Int startPosition = Vector3Int.zero;
        System.Random random = new System.Random();
        //grassRoots = new List<Vector3>();
        grassRootsDir = new List<Vector4>();
        grassHeights = new List<float>();
        grassDensityIndexes = new List<float>();

        //随机生成草根位置、方向、高度、密度索引
        for (int i = 0; i < pregenerateGrassAmount; i++) {
            float deltaX = (float)random.NextDouble();
            float deltaZ = (float)random.NextDouble();
            Vector3 root = new Vector3(deltaX * PATCH_SIZE, 0, deltaZ * PATCH_SIZE);

            //grassRoots.Add(root);
            grassRootsDir.Add(new Vector4(root.x, root.y, root.z, (float)random.NextDouble()));
            grassHeights.Add(grassHeightMax * 0.5f + grassHeightMax * 0.5f * (float)random.NextDouble());
            grassDensityIndexes.Add((float)random.NextDouble());
        }

        //send to gpu
        grassMaterial.SetVectorArray(Shader.PropertyToID("_patchRootsPosDir"), grassRootsDir);
        grassMaterial.SetFloatArray(Shader.PropertyToID("_patchGrassHeight"), grassHeights);
        grassMaterial.SetFloatArray(Shader.PropertyToID("_patchDensities"), grassDensityIndexes);
    }

    /// <summary>
    /// 更新CS的RT(frustumTexture),
    /// grassMaterial的_FrustumStartPosI和instanceBound
    /// </summary>
    /// <param name="camera"></param>
    public void UpdateFrustumComputeShader(Camera camera) {
        Vector3[] frustum = new Vector3[3];
        //想传6个整数，但unity的computeShader.SetInts和SetFloats有问题
        Vector4[] frusIndex = { Vector4.zero, Vector4.zero };
        #region 获取视锥体frustum
        float halfFOV = (camera.fieldOfView * 0.5f) * Mathf.Deg2Rad;
        float height = camera.farClipPlane * Mathf.Tan(halfFOV);
        float width = height * camera.aspect;

        Vector3 vec = camera.transform.position, widthDelta = camera.transform.right * width;
        vec -= widthDelta;
        vec += (camera.transform.forward * camera.farClipPlane);
        frustum[0] = camera.transform.position; frustum[1] = vec; frustum[2] = vec + 2 * widthDelta;
        #endregion
        Bounds camBound = new Bounds(frustum[0], Vector3.zero);//相机包围盒
        for (int i = 1; i < frustum.Length; i++) {
            camBound.Encapsulate(frustum[i]);
        }
        Vector2Int minIndex = tBuilder.GetConstrainedTileIndex(camBound.min),
            maxIndex = tBuilder.GetConstrainedTileIndex(camBound.max);

        //更新compute shader
        int digit = 0, newTexSizeX, newTexSizeY;
        #region 将frustumTexSize向上取整（二进制），eg: 9→16
        newTexSizeX = maxIndex.x - minIndex.x + 1;
        for (int i = 31; i >= 0; i--)
            if (((newTexSizeX >> i) & 1) == 1) { digit = i + 1; break; }
        newTexSizeX = 1 << digit;//向上取整（二进制），eg: 9→16
        newTexSizeY = maxIndex.y - minIndex.y + 1;
        for (int i = 31; i >= 0; i--)
            if (((newTexSizeY >> i) & 1) == 1) { digit = i + 1; break; }
        newTexSizeY = 1 << digit;
        #endregion
        if (newTexSizeX != frustumTexSizeX || newTexSizeY != frustumTexSizeY) {
            //new testTexture texture
            //for test
            frustumTexture = new RenderTexture(newTexSizeX, newTexSizeY, 24);
            frustumTexture.enableRandomWrite = true;
            frustumTexture.Create();
            frustumCalcShader.SetTexture(frustumKernel, "testTexture", frustumTexture);
            plane.GetComponent<Renderer>().sharedMaterial.mainTexture = frustumTexture;
        }
        frustumTexSizeX = newTexSizeX; frustumTexSizeY = newTexSizeY;

        for (int i = 0; i < 3; i++) {
            Vector2Int t = tBuilder.GetTileIndex(frustum[i]);
            t -= tBuilder.GetTileIndex(camBound.min);
            frusIndex[i / 2] += new Vector4(t.x * Mathf.Abs(i - 1), t.y * Mathf.Abs(i - 1),
                t.x * (i % 2), t.y * (i % 2));
        }
        //Debug.Log(frusIndex[0].ToString() + "   " + frusIndex[1].ToString());
        instanceBound = camBound;
        frustumCalcShader.SetVectorArray(Shader.PropertyToID("frustumPosIndex"), frusIndex);
        Shader.SetGlobalVector(Shader.PropertyToID("threadSize"),
            new Vector4(frustumTexSizeX, frustumTexSizeY, 0, 0));
        var f = tBuilder.GetTileIndex(camBound.min);
        Shader.SetGlobalVector(Shader.PropertyToID("_FrustumStartPosI"),
            new Vector4(f.x, f.y));

    }


    void RunComputeShader() {
        //运行shader  参数1=kid  参数2=线程组在x维度的数量 参数3=线程组在y维度的数量 参数4=线程组在z维度的数量
        frustumCalcShader.Dispatch(frustumKernel, frustumTexSizeX / threadGroupSizeX,
            frustumTexSizeY / threadGroupSizeY, 1);
        //Debug.Log(frustumTexSizeX / threadGroupSizeX + "   " +frustumTexSizeY / threadGroupSizeY);
    }

    /// <summary>
    /// generate a mesh with assigned numbers of points
    /// </summary>
    Mesh generateGrassTile(int grassBladeCount) {
        Mesh result = new Mesh();
        int bladeVertexCount = (bladeSectionCount + 1) * 2;
        Vector3[] normals = new Vector3[grassBladeCount * bladeVertexCount];
        Vector3[] vertices = new Vector3[grassBladeCount * bladeVertexCount];
        Vector2[] uv = new Vector2[grassBladeCount * bladeVertexCount];
        for (int i = 0; i < vertices.Length; i++) {
            //赋予x坐标，为了使其作为索引在gpu中读取数组信息
            vertices[i] = new Vector3(i / bladeVertexCount, i % bladeVertexCount, 0);//0-63,0-11,0
            normals[i] = -Vector3.forward;
            uv[i] = new Vector2(i % bladeVertexCount % 2,
                ((float)(i % bladeVertexCount / 2)) / bladeSectionCount);
        }
        result.vertices = vertices;

        int[] triangles = new int[6 * grassBladeCount * bladeSectionCount];
        int trii = 0;
        for (int blade = 0; blade < grassBladeCount; blade++) {
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



    /// <summary>
    /// 更新GPU端的_tileHeightDeltaStartIndex和CPU端的matrices
    /// </summary>
    /// <param name="tiles"></param>
    /// <returns></returns>
    /*MaterialPropertyBlock UpdateGrassInfo(List<Vector2Int> tiles) {
        prop = new MaterialPropertyBlock();
        matrices = new Matrix4x4[tiles.Count];
        propData = new Vector4[tiles.Count];
        for (int i = 0; i < matrices.Length; i++) {
            //calculate transform matrix
            matrices[i] = Matrix4x4.TRS(tBuilder.GetTilePosition(tiles[i])
                , Quaternion.identity, Vector3.one);
            //calculate instance property
            int x = tiles[i].x, y = tiles[i].y;
            float originY = tBuilder.GetTilePosition(tiles[i]).y;
            //根据tile的行、列号的伪随机，保证每个tile渲染时在patch的起始位置都是不变的
            float random = (float)
                ((new System.Random(x).NextDouble()) + (new System.Random(y).NextDouble())) / 2f;
            int index = (int)(random * (pregenerateGrassAmount - grassAmountPerTile));
            propData[i] = new Vector4(
                tBuilder.GetTilePosition(x + 1, y).y - originY,
                tBuilder.GetTilePosition(x + 1, y + 1).y - originY,
                tBuilder.GetTilePosition(x, y + 1).y - originY,
                index);
            //Debug.Log(propData[i]);
        }
        prop.SetVectorArray("_tileHeightDeltaStartIndex", propData);
        return prop;
    }*/

    void Start() {
        grassMaterial.SetInt("_SectionCount", bladeSectionCount);
        tBuilder = GameObject.Find("terrain").GetComponent<TerrainBuilder>();
        grassMesh = generateGrassTile(grassAmountPerTile);
        //draw indirect arguments
        uint meshIndicesNum = (uint)grassMesh.vertices.Length;//grassMesh.GetIndexCount(0);
        uint[] args = new uint[5] { meshIndicesNum, 0, 0, 0, 0 };
        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint),
            ComputeBufferType.IndirectArguments);
        frustumCalcShader.SetBuffer(frustumKernel, "indirectDataBuffer", argsBuffer);

        ///grass
        GameObject grass = new GameObject("grass", typeof(MeshRenderer), typeof(MeshFilter));
        grass.transform.parent = transform;
        grass.GetComponent<MeshFilter>().mesh = grassMesh;
        grass.GetComponent<MeshRenderer>().sharedMaterial = grassMaterial;

        PregenerateGrassInfo();
        //渲染计算
        /*uint x, y, z;
        frustumCalcShader.GetKernelThreadGroupSizes(frustumKernel,out x, out y, out z);//8,8,1
        Debug.Log(x + " " + y + " " + z);*/
    }

    private void Update() {
        //视锥体计算
        UpdateFrustumComputeShader(Camera.main);
        RunComputeShader();

        plane.transform.localScale =
            new Vector3(PATCH_SIZE * frustumTexSizeX,
            PATCH_SIZE * frustumTexSizeY, 1);
        plane.transform.position = instanceBound.min +
            new Vector3(PATCH_SIZE * frustumTexSizeX / 2, 0, PATCH_SIZE * frustumTexSizeY / 2);

        //渲染
        //render grass,TODO: LOD 64 32 16
        Graphics.DrawMeshInstancedIndirect(grassMesh,
            0, grassMaterial, instanceBound, argsBuffer);
        
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.green;
        /*for (int i = 0; i < tilesToRender.Count; i++) {
            Gizmos.color = new Color(
                i % 3 == 0 ? i / 1: 0,
                i % 3 == 1 ? i / 1: 0,
                i % 3 == 2 ? i / 1: 0
                );
            Gizmos.DrawSphere(testData[i].a, 0.3f);
            Gizmos.DrawSphere(testData[i].b, 0.3f);
            Gizmos.DrawSphere(testData[i].c, 0.3f);
            Gizmos.DrawSphere(testData[i].d, 0.3f);
        }*/
        Gizmos.DrawWireCube(instanceBound.center, instanceBound.size);
    }
}
