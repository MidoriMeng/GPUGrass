using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RealtimeLawnSystem : MonoBehaviour {
    //frustum
    public ComputeShader calcShader;
    //grass
    public Texture2D grassDensityMap;
    public int grassAmountPerTile = 64;//实际渲染时每个Tile内最多的草叶数量
    public int pregenerateGrassAmount = 1023;//预生成Patch草体总长度
    public Material grassMaterial;
    //terrain
    public Texture2D heightMap;
    public float terrainHeight = 5f;
    public const int PATCH_SIZE = 2;//Patch的边长
    public Material terrainMat;

    private GrassGenerator grassGen;
    private TerrainBuilder terrainBuilder;
    private FrustumCalculation frustumCalc;
    private Mesh grassMesh;

    //indirect
    private ComputeBuffer argsBuffer;
    private Bounds instanceBound;

    void Awake() {
        //地形
        terrainBuilder = new TerrainBuilder(heightMap, terrainHeight, terrainMat);
        GameObject terrain = GameObject.Find("terrain");
        if (!terrain)
            terrainBuilder.BuildTerrain(transform);
        terrainBuilder.BuildTerrainDataBuffer();
        //草叶
        grassGen = new GrassGenerator(grassDensityMap, grassAmountPerTile,
            pregenerateGrassAmount, grassMaterial,TerrainBuilder.PATCH_SIZE);
        grassMesh = grassGen.generateGrassTile();
        grassGen.PregenerateGrassInfo();
        //视锥体
        frustumCalc = new FrustumCalculation(calcShader, terrainBuilder);

        //draw indirect arguments
        uint meshIndicesNum = (uint)grassMesh.vertices.Length;//grassMesh.GetIndexCount(0);
        uint[] args = new uint[5] { meshIndicesNum, 0, 0, 0, 0 };
        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint),
            ComputeBufferType.IndirectArguments);
        //frustumCalcShader.SetBuffer(frustumKernel, "indirectDataBuffer", argsBuffer);
        Shader.SetGlobalBuffer("indirectDataBuffer", argsBuffer);
    }

    void Update() {
        //视锥体计算
        instanceBound = frustumCalc.Run(Camera.main);
        frustumCalc.forDebug();

        //render grass,TODO: LOD 64 32 16
        /*Graphics.DrawMeshInstancedIndirect(grassMesh,
            0, grassGen.grassMaterial, instanceBound, argsBuffer);*/

        /*uint x, y, z;//test
        frustumCalcShader.GetKernelThreadGroupSizes(frustumKernel,out x, out y, out z);//8,8,1
        Debug.Log(x + " " + y + " " + z);*/
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(instanceBound.center, instanceBound.size);
    }

    public void BuildTerrainTool() {
        terrainBuilder = new TerrainBuilder(heightMap, terrainHeight, terrainMat);
        GameObject terrain = GameObject.Find("terrain");
        if(terrain)
            DestroyImmediate(terrain);
        terrainBuilder.BuildTerrain(transform);
    }
}
