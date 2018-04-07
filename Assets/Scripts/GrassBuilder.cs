using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class GrassBuilder : MonoBehaviour {
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
    public float patchExpansion = 2f;//实际渲染时比视锥体范围扩展的距离，为了保证边界渲染的质量且减少视锥体裁剪的频率

    private List<float> grassHeights;
    private List<float> grassDensityIndexes;
    //private List<Vector3> grassRoots;//草的位置
    private List<Vector4> grassRootsDir;//草的位置+随机方向，[0,1]

    private List<Vector2Int> tilesToRender;
    private Matrix4x4[] matrices;
    private MaterialPropertyBlock prop;

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

    public List<Vector2Int> calculateTileToRender() {
        //获取相机视锥体在世界坐标系下的包围盒
        Camera camera = Camera.main;
        Vector3[] frustumCorners = new Vector3[4];//临时变量，存本地坐标下相机frustum五个角
        camera.CalculateFrustumCorners(new Rect(0, 0, 1, 1), camera.farClipPlane, camera.stereoActiveEye, frustumCorners);
        List<Vector3> frustumBoundPoints = new List<Vector3>();//临时变量，存世界坐标下的五个角
        for (int i = 0; i < 4; i++) {
            Vector3 worldSpaceCorner = camera.transform.TransformVector(frustumCorners[i]);
            frustumBoundPoints.Add(camera.transform.position + worldSpaceCorner);
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
        Bounds camBound = new Bounds(frustumTriangle[0], Vector3.zero);//相机包围盒
        for (int i = 1; i < frustumTriangle.Length; i++) {
            camBound.Encapsulate(frustumTriangle[i]);
        }
        //Debug.Log(camBound.min + "  " + camBound.max);

        //确定检测范围
        //包围盒内的tile依次与相机求交，获得需要渲染的tile，储存在tilesToRender
        List<Vector2Int> result = new List<Vector2Int>();
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
                if (PointInTriangle(frustumTriangle[0], frustumTriangle[1], frustumTriangle[2], testBound.center)) {
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
                if (PointInTriangle(frustumTriangle[0], frustumTriangle[1], frustumTriangle[2], testBound.center)) {
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
        int bladeVertexCount = (bladeSectionCount + 1) * 2;
        Vector3[] normals = new Vector3[grassBladeCount * bladeVertexCount];
        Vector3 []vertices = new Vector3[grassBladeCount * bladeVertexCount];
        Vector2[] uv = new Vector2[grassBladeCount * bladeVertexCount];
        for(int i = 0; i < vertices.Length; i++) {
            //赋予x坐标，为了使其作为索引在gpu中读取数组信息
            //vertices[i].x = i / bladeVertexCount;//0~63
            //vertices[i].y = (int)(i % bladeVertexCount);//0~11
            vertices[i] = new Vector3((i % bladeVertexCount % 2 + (int)(i/bladeVertexCount)*2),
                (int)(i % bladeVertexCount / 2) + i / bladeVertexCount * 100,//0~63*100
                i % bladeVertexCount);//0~11
            normals[i] = -Vector3.forward;
            uv[i] = new Vector2(i % bladeVertexCount % 2,
                ((float)(i % bladeVertexCount / 2)) / bladeSectionCount);
        }
        result.vertices = vertices;

        int[] triangles = new int[6 * grassBladeCount * bladeSectionCount];
        int trii = 0;
        for(int blade=0;blade< grassBladeCount; blade++) {
            for(int section = 0; section < bladeSectionCount; section++) {
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

    /*public MaterialPropertyBlock GeneratePropertyBlock(List<Vector2Int> tilesToRender) {
        System.Random random = new System.Random();
        MaterialPropertyBlock props = new MaterialPropertyBlock();

        for (int i = 0; i < tilesToRender.Count; i++) {
            //TileBuffer tempTile = new TileBuffer();
            float originY = tBuilder.GetTilePosition(tilesToRender[i]).y;
            int index = (int)(random.NextDouble() * (pregenerateGrassAmount - grassAmountPerTile));
            //tempTile.worldCoordinateStartIndex = new Vector4(pos.x, pos.y, pos.z, index);
            //float originHeight = tempTile.worldCoordinateStartIndex.y;
            int x = tilesToRender[i].x, y = tilesToRender[i].y;

            //send data to property block
            props.SetVector("_tileHeightDeltaStartIndex", new Vector4(
                tBuilder.GetTilePosition(x + 1, y).y - originY,
                tBuilder.GetTilePosition(x + 1, y + 1).y - originY,
                tBuilder.GetTilePosition(x, y + 1).y - originY,
                index
                )
            );

        }
        return props;
    }*/

    /// <summary>
    /// xz平面中点p是否在abc构成的三角形内
    /// </summary>
    public bool PointInTriangle(Vector3 a, Vector3 b, Vector3 c, Vector3 p) {
        float v2x = (p - a).x, v2y = (p - a).z;
        float v0x = (c - a).x, v0y = (c - a).z;
        float v1x = (b - a).x, v1y = (b - a).z;
        float v = (v0x * v2y - v0y * v2x) / (v0x * v1y - v0y * v1x);
        float u = (v1x * v2y - v1y * v2x) / (v0y * v1x - v0x * v1y);
        if (u < 0 || u > 1) // if u out of range, return directly
            return false;

        if (v < 0 || v > 1) // if v out of range, return directly
            return false;
        return u + v <= 1;
    }



    MaterialPropertyBlock UpdateGrassInfo(List<Vector2Int> tiles) {
        prop = new MaterialPropertyBlock();
        matrices = new Matrix4x4[tiles.Count];
        Vector4[] propData = new Vector4[tiles.Count];
        System.Random random = new System.Random();
        for (int i = 0; i < matrices.Length; i++) {
            //calculate transform matrix
            matrices[i] = Matrix4x4.TRS(tBuilder.GetTilePosition(tiles[i])
                , Quaternion.identity, Vector3.one);
            //calculate instance property
            float originY = tBuilder.GetTilePosition(tiles[i]).y;
            int index = (int)(random.NextDouble() * (pregenerateGrassAmount - grassAmountPerTile));
            int x = tiles[i].x, y = tiles[i].y;
            propData[i] = new Vector4(
                tBuilder.GetTilePosition(x + 1, y).y - originY,
                tBuilder.GetTilePosition(x + 1, y + 1).y - originY,
                tBuilder.GetTilePosition(x, y + 1).y - originY,
                index);
            //Debug.Log(propData[i]);
        }
        prop.SetVectorArray("_tileHeightDeltaStartIndex", propData);
        return prop;
    }

    void Start() {
        grassMaterial.SetInt("_SectionCount", bladeSectionCount);
        tBuilder = GameObject.Find("terrain").GetComponent<TerrainBuilder>();
        grassMesh = generateGrassTile(64);
        ///grass
        GameObject grass = new GameObject("grass", typeof(MeshRenderer),typeof(MeshFilter));
        grass.transform.parent = transform;
        grass.GetComponent<MeshFilter>().mesh = grassMesh;
        grass.GetComponent<MeshRenderer>().sharedMaterial = grassMaterial;

        PregenerateGrassInfo();
        tilesToRender = calculateTileToRender();
        prop = UpdateGrassInfo(tilesToRender);
    }

    private void Update() {
        //for test
        //send to gpu
        grassMaterial.SetVectorArray(Shader.PropertyToID("_patchRootsPosDir"), grassRootsDir);
        grassMaterial.SetFloatArray(Shader.PropertyToID("_patchGrassHeight"), grassHeights);
        grassMaterial.SetFloatArray(Shader.PropertyToID("_patchDensities"), grassDensityIndexes);
        //render grass,TODO: LOD 64 32 16
        Graphics.DrawMeshInstanced(grassMesh, 0, grassMaterial, matrices, matrices.Length, prop);

    }

    /*private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        for (int i = 0; i < tilesToRender.Count; i++) {
            var position = tBuilder.GetTilePosition(tilesToRender[i]);
            position.y = 100f;
            Gizmos.DrawSphere(
                position, 0.1f);
        }

    }*/
}
