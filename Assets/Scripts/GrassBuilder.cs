using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrassBuilder : MonoBehaviour {
    private TerrainBuilder tBuilder;
    //grass
    public Texture2D grassDensityMap;
    public float grassHeightMax = 2f;
    public const int PATCH_SIZE = 1;//Patch的长/宽
    public int grassAmountPerTile;//实际渲染时每个Tile内最多的草叶数量
    public int pregenerateGrassAmount = 1024;//预生成Patch草体总长度
    public Material grassMaterial;
    public float patchExpansion = 2f;//实际渲染时比视锥体范围扩展的距离，为了保证边界渲染的质量且减少视锥体裁剪的频率
    private List<float> grassHeights;
    private List<float> grassDensityIndexes;
    //private List<Vector3> grassRoots;//草的位置
    private List<Vector4> grassRootsDir;//草的位置+随机方向，[0,1]
    private GameObject grassLayer;//草体模型所在gameObject

    private List<Vector2Int> tilesToRender;

    /// <summary>
    /// 预生成草地信息数组，传输给grassMaterial
    /// </summary>
    public void PregenerateGrass() {
        grassLayer = GameObject.Find("GrassLayer");
        if (grassLayer)
            DestroyImmediate(grassLayer);
        Vector3Int startPosition = Vector3Int.zero;//待定
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

        //生成草地模型
        /*Mesh m = new Mesh();
        m.vertices = grassRoots.ToArray();
        int[] indices = new int[grassRoots.Count];
        for (int i = 0; i < indices.Length; i++)
            indices[i] = i;
        m.SetIndices(indices, MeshTopology.Points, 0);
        grassLayer = (new GameObject("GrassLayer"));
        grassLayer.transform.parent = gameObject.transform;
        MeshFilter filter = grassLayer.gameObject.AddComponent<MeshFilter>();
        MeshRenderer renderer = grassLayer.gameObject.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = grassMaterial;
        filter.mesh = m;*/
        //send to gpu
        grassMaterial.SetVectorArray(Shader.PropertyToID("_patchRootsPosDir"), grassRootsDir);
        grassMaterial.SetFloatArray(Shader.PropertyToID("_patchGrassHeight"), grassHeights);
        grassMaterial.SetFloatArray(Shader.PropertyToID("_patchDensities"), grassDensityIndexes);
    }

    public List<Vector2Int> calculateTileToRender() {
        //获取相机视锥体在世界坐标系下的包围盒
        Camera camera = Camera.main;
        Vector3[] frustumCorners = new Vector3[4];//临时变量，存本地坐标下相机frustum五个角
        camera.CalculateFrustumCorners(new Rect(0, 0, 1, 1), camera.farClipPlane, camera.stereoActiveEye, frustumCorners);
        List<Vector3> frustumBoundPoints = new List<Vector3>();//临时变量，存世界坐标下的五个角
        for (int i = 0; i < 4; i++) {
            Vector3 worldSpaceCorner = camera.transform.TransformVector(frustumCorners[i]);
            frustumBoundPoints.Add(worldSpaceCorner);
        }
        frustumBoundPoints.Add(camera.transform.position);
        //视锥体在xz平面上的三角形
        Vector3[] frustumTriangle = new Vector3[] { frustumBoundPoints[0], frustumBoundPoints[2], frustumBoundPoints[4] };
        //为了显示效果，边界向外扩张两个patchSize大小
        Vector3 triangleCenter = (frustumTriangle[0] + frustumTriangle[1] + frustumTriangle[2]) / 3f;
        frustumTriangle[0] += patchExpansion * PATCH_SIZE * (frustumTriangle[0] - triangleCenter)
            / (frustumTriangle[0] - triangleCenter).magnitude;
        frustumTriangle[1] += patchExpansion * PATCH_SIZE * (frustumTriangle[1] - triangleCenter)
            / (frustumTriangle[1] - triangleCenter).magnitude;
        frustumTriangle[2] += patchExpansion * PATCH_SIZE * (frustumTriangle[2] - triangleCenter)
            / (frustumTriangle[2] - triangleCenter).magnitude;
        Bounds camBound = new Bounds();//相机包围盒
        for (int i = 0; i < frustumTriangle.Length; i++) {
            camBound.Encapsulate(frustumTriangle[i]);
        }
        //Debug.Log(camBound.min + "  " + camBound.max);

        //确定检测范围
        //包围盒内的tile依次与相机求交，获得需要渲染的tile，储存在tilesToRender
        List<Vector2Int>  result = new List<Vector2Int>();
        Vector2Int minIndex = tBuilder.GetTileIndex(camBound.min), maxIndex = tBuilder.GetTileIndex(camBound.max);
        minIndex = tBuilder.GetConstrainedTileIndex(minIndex.x, minIndex.y);
        maxIndex = tBuilder.GetConstrainedTileIndex(maxIndex.x, maxIndex.y);
        Bounds testBound = new Bounds(
                    tBuilder.GetTilePosition(minIndex) + new Vector3(0.5f * PATCH_SIZE, 0, 0.5f * PATCH_SIZE),
                    new Vector3(PATCH_SIZE, PATCH_SIZE, PATCH_SIZE));//patch的包围盒，用来一边移动一边与frustum求交
        //求交。检测对象：每个tile与frustum；方法：扫描线检测法
        float iterationStartZ = testBound.center.z;
        for (int i = minIndex.x; i <= maxIndex.x; i++, testBound.center += new Vector3(PATCH_SIZE, 0, 0)) {
            //在z方向上先从min向max找到第一个相交patch：minZ，
            int minZ = 0;
            bool finded = false;
            for (int j = minIndex.y; j <= maxIndex.y; j++, testBound.center += new Vector3(0, 0, PATCH_SIZE)) {
                if (tBuilder.PointInTriangle(frustumTriangle[0], frustumTriangle[1], frustumTriangle[2], testBound.center)) {
                    finded = true;
                    minZ = j;
                    break;
                }
            }
            if (!finded) {//正向没找到，下一个。TODO：删掉，不应该有找不到的
                testBound.center = new Vector3(testBound.center.x, testBound.center.y, iterationStartZ);//z复位
                continue;
            }
            //再按反方向找第一个相交patch：maxZ，
            int maxZ = 0;
            float newZ = tBuilder.GetTilePosition(minIndex.x, maxIndex.y).z + 0.5f * PATCH_SIZE;
            testBound.center = new Vector3(testBound.center.x, testBound.center.y, newZ);
            for (int j = maxIndex.y; j >= minIndex.y; j--, testBound.center -= new Vector3(0, 0, PATCH_SIZE)) {
                if (tBuilder.PointInTriangle(frustumTriangle[0], frustumTriangle[1], frustumTriangle[2], testBound.center)) {
                    maxZ = j;
                    break;
                }
            }
            //则patchMin和patchMax以及之间的patch都在视锥体范围内，存入tilesToRender
            //Debug.Log("min:" + minZ + "; max:" + maxZ);
            for (int j = minZ; j <= maxZ; j++) {
                result.Add(new Vector2Int(i, j));
            }
            //Debug.Log(tilesToRender.Count);

            if (result.Count > 256)
                Debug.LogError(">256, tileData buffer overflow");
            testBound.center = new Vector3(testBound.center.x, testBound.center.y, iterationStartZ);//z复位
        }
        return result;
    }

    /// <summary>
    /// generate a mesh with assigned numbers of points
    /// </summary>
    /// <param name="grassBladeCount"></param>
    /// <returns></returns>
    Mesh generateGrassTile(int grassBladeCount) {
        Mesh result = new Mesh();
        //set mesh vertices
        result.vertices = new Vector3[grassBladeCount];
        //set mesh indices
        int[] indices = new int[grassBladeCount];
        for (int i = 0; i < grassBladeCount; i++)
            indices[i] = i;
        result.SetIndices(indices, MeshTopology.Points, 0);

        return result;
    }

    // Use this for initialization
    void Start () {
        tBuilder = GameObject.Find("terrain").GetComponent<TerrainBuilder>();

        PregenerateGrass();
        tilesToRender = calculateTileToRender();
        MaterialPropertyBlock props = tBuilder.GeneratePropertyBlock(
            tilesToRender, pregenerateGrassAmount, grassAmountPerTile);
        ///GetComponent<MeshRenderer>().SetPropertyBlock(props);

    }

    private void Update() {
        //render grass,TODO: LOD 64 32 16
        Mesh grassTile = generateGrassTile(64);
        var position = Vector3.zero;
        var rotation = Quaternion.identity;
        var scale = Vector3.one;
        Matrix4x4[] matrices = new Matrix4x4[tilesToRender.Count];
        for (int i = 0; i < matrices.Length; i++) {
            position = tBuilder.GetTilePosition(tilesToRender[i]);
            matrices[i] = Matrix4x4.TRS(position, rotation, scale);
        }
        Graphics.DrawMeshInstanced(grassTile, 0, grassMaterial, matrices);
    }
}
