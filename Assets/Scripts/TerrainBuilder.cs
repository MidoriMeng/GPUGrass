using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainBuilder : MonoBehaviour {
    //terrain
    public Texture2D heightMap;
    public float terrainHeight = 5f;
    public float terrainScale = 1f;
    public Material terrainMat;
    private List<Vector3> vertices;
    //grass
    public Texture2D grassDensityMap;
    public float grassHeightMax = 2f;
    public int patchSize = 1;//Patch的长/宽
    public int grassAmountPerTile;//实际渲染时每个Tile内最多的草叶数量
    public int pregenerateGrassAmount = 1024;//预生成Patch草体总长度
    public Material grassMaterial;
    public float patchExpansion = 2f;//实际渲染时比视锥体范围扩展的距离，为了保证边界渲染的质量且减少视锥体裁剪的频率
    private List<float> directions;//随机方向，[0,1]
    private List<float> grassHeights;
    private List<float> grassDensityIndexes;
    private List<Vector3> grassRoots;//草的位置
    private Transform grassLayer;//草体模型所在gameObject
    private TileBuffer []tileData;

    private List<Vector2Int> tilesToRender;

    public struct TileBuffer {
        public Vector3 worldCoordinate;
        public Vector3 heightDelta;
        public int startIndexAtPatch;
    };

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

    public void PregenerateGrass() {
        if (grassLayer)
            DestroyImmediate(grassLayer.gameObject);
        Vector3Int startPosition = Vector3Int.zero;//待定
        System.Random random = new System.Random();
        grassRoots = new List<Vector3>();
        directions = new List<float>();
        grassHeights = new List<float>();
        grassDensityIndexes = new List<float>();

        //随机生成草根位置、方向、高度、密度索引
        for (int i = 0; i < pregenerateGrassAmount; i++) {
            float deltaX = (float)random.NextDouble();
            float deltaZ = (float)random.NextDouble();
            grassRoots.Add(new Vector3(deltaX * patchSize, 0, deltaZ * patchSize));
            directions.Add((float)random.NextDouble());
            grassHeights.Add(grassHeightMax * 0.5f + grassHeightMax * 0.5f * (float)random.NextDouble());
            grassDensityIndexes.Add((float)random.NextDouble());
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
    }


    public void calculateTileToRender() {
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
        frustumTriangle[0] += patchExpansion * patchSize * (frustumTriangle[0] - triangleCenter) 
            / (frustumTriangle[0] - triangleCenter).magnitude;
        frustumTriangle[1] += patchExpansion * patchSize * (frustumTriangle[1] - triangleCenter)
            / (frustumTriangle[1] - triangleCenter).magnitude;
        frustumTriangle[2] += patchExpansion * patchSize * (frustumTriangle[2] - triangleCenter)
            / (frustumTriangle[2] - triangleCenter).magnitude;
        Bounds camBound = new Bounds();//相机包围盒
        for (int i = 0; i < frustumTriangle.Length; i++) {
            camBound.Encapsulate(frustumTriangle[i]);
        }

        //确定检测范围
        //包围盒内的tile依次与相机求交，获得需要渲染的tile，储存在tilesToRender
        tilesToRender = new List<Vector2Int>();
        Vector2Int minIndex = GetTileIndex(camBound.min), maxIndex = GetTileIndex(camBound.max);
        minIndex = GetConstrainedTileIndex(minIndex.x, minIndex.y);
        maxIndex = GetConstrainedTileIndex(maxIndex.x, maxIndex.y);

        Bounds testBound = new Bounds(
                    GetTilePosition(minIndex) + new Vector3(0.5f * patchSize, 0, 0.5f * patchSize),
                    new Vector3(patchSize, patchSize, patchSize));//patch的包围盒，用来一边移动一边与frustum求交
        //求交。检测对象：每个tile与frustum；方法：扫描线检测法
        float iterationStartZ = testBound.center.z;
        for (int i = minIndex.x; i <= maxIndex.x; i++, testBound.center += new Vector3(patchSize, 0, 0)) {
            //在z方向上先从min向max找到第一个相交patch：minZ，
            int minZ = 0;
            bool finded = false;
            for (int j = minIndex.y; j <= maxIndex.y; j++, testBound.center += new Vector3(0, 0, patchSize)) {
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
            float newZ = GetTilePosition(minIndex.x, maxIndex.y).z + 0.5f * patchSize;
            testBound.center = new Vector3(testBound.center.x, testBound.center.y, newZ);
            for (int j = maxIndex.y; j >= minIndex.y; j--, testBound.center -= new Vector3(0, 0, patchSize)) {
                if (PointInTriangle(frustumTriangle[0], frustumTriangle[1], frustumTriangle[2], testBound.center)) {
                    maxZ = j;
                    break;
                }
            }
            //则patchMin和patchMax以及之间的patch都在视锥体范围内，存入tilesToRender
            //Debug.Log("min:" + minZ + "; max:" + maxZ);
            for (int j = minZ; j <= maxZ; j++) {
                tilesToRender.Add(new Vector2Int(i, j));
                //Debug.Log(new Vector2(i, j));
            }
            //Debug.Log(tilesToRender.Count);

            if (tilesToRender.Count > 256)
                Debug.LogError(">256, tileData buffer overflow");
            testBound.center = new Vector3(testBound.center.x, testBound.center.y, iterationStartZ);//z复位
        }

        GenerateTileData();
    }

    void GenerateTileData() {
        tileData = new TileBuffer[256];
        System.Random random = new System.Random();
        for (int i = 0; i < tilesToRender.Count; i++) {
            TileBuffer tempTile = new TileBuffer();
            tempTile.worldCoordinate = GetTilePosition(tilesToRender[i]);
            float originHeight = tempTile.worldCoordinate.y;
            int x = tilesToRender[i].x, y = tilesToRender[i].y;
            Debug.Log(x + "  " + y + "  " + vertices.Count);
            tempTile.heightDelta = new Vector3(
                vertices[heightMap.width * (x + 1) + y].y - originHeight,
                vertices[heightMap.width * (x + 1) + y + 1].y - originHeight,
                vertices[heightMap.width * x + y + 1].y - originHeight
                );
            tempTile.startIndexAtPatch = (int)(random.NextDouble() * (pregenerateGrassAmount - grassAmountPerTile));
            tileData[i] = tempTile;
        }
    }
    /// <summary>
    /// xz平面中点p是否在abc构成的三角形内
    /// </summary>
    bool PointInTriangle(Vector3 a, Vector3 b, Vector3 c, Vector3 p) {
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

    /// <summary>
    /// 从vertices读取位置
    /// </summary>
    Vector3 GetTilePosition(int i, int j) {
        Vector2Int index = GetConstrainedTileIndex(i, j);
        return vertices[index.x * heightMap.width + index.y];
    }

    Vector2Int GetConstrainedTileIndex(int indexX, int indexZ) {
        indexX = Mathf.Max(0, indexX); indexX = Mathf.Min(indexX, heightMap.width - 2);
        indexZ = Mathf.Max(0, indexZ); indexZ = Mathf.Min(indexZ, heightMap.height - 2);
        return new Vector2Int(indexX, indexZ);
    }

    Vector3 GetTilePosition(Vector2Int index) {
        return GetTilePosition(index.x, index.y);
    }

    Vector2Int GetTileIndex(Vector3 position) {
        return new Vector2Int(Mathf.FloorToInt(position.x / patchSize), Mathf.FloorToInt(position.z / patchSize));
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
