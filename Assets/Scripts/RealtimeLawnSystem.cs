using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RealtimeLawnSystem : MonoBehaviour {
    //all
    private int maxTileRenderCount = 1024;
    private ComputeBuffer sizeBuffer;
    private ComputeBuffer renderPosAppendBuffer;
    private ComputeBuffer counterBuffer;
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
    public Mesh grassMesh;//for test应该改为private

    //indirect
    private ComputeBuffer argsBuffer;
    private Bounds instanceBound;

    //test
    Vector3[] poses;
    public Material testMat;

    void Awake() {
        sizeBuffer = new ComputeBuffer(6, sizeof(float));
        float[] sizeBufferData = new float[6];
        sizeBuffer.SetData(sizeBufferData);
        Shader.SetGlobalBuffer("sizeBuffer", sizeBuffer);
        //地形
        terrainBuilder = new TerrainBuilder(heightMap, terrainHeight, terrainMat);
        GameObject terrain = GameObject.Find("terrain");
        if (!terrain)
            terrainBuilder.BuildTerrain(transform);
        //草叶
        grassGen = new GrassGenerator(grassDensityMap, grassAmountPerTile,
            pregenerateGrassAmount, grassMaterial, TerrainBuilder.PATCH_SIZE);
        grassMesh = grassGen.generateGrassTile();
        grassGen.PregenerateGrassInfo();
        //视锥体
        frustumCalc = new FrustumCalculation(calcShader, terrainBuilder);
        //terrain data buffer
        var terrainBuffer = terrainBuilder.BuildTerrainDataBuffer();
        frustumCalc.SetBuffer("terrainDataBuffer", terrainBuffer);
        Shader.SetGlobalBuffer("terrainDataBuffer", terrainBuffer);
        grassMaterial.SetInt("grassAmountPerTile", grassAmountPerTile);
        grassMaterial.SetInt("pregenerateGrassAmount", pregenerateGrassAmount);

        testMat.SetInt("grassAmountPerTile", grassAmountPerTile);
        testMat.SetInt("pregenerateGrassAmount", pregenerateGrassAmount);

        //test
        testMat.SetBuffer("renderPosAppend", renderPosAppendBuffer);
    }

    void Update() {
        //视锥体计算
        Vector4 frustumSize;
        instanceBound = frustumCalc.PrepareCamData(Camera.main, out frustumSize);

        //更新sizeBuffer:场景大小、检测面积大小、检测起始点
        float[] sizeBufferData = { heightMap.width,heightMap.height,
        frustumSize.x,frustumSize.y,frustumSize.z,frustumSize.w};
        if (sizeBuffer != null)
            sizeBuffer.Release();
        sizeBuffer = new ComputeBuffer(6, sizeof(float));
        sizeBuffer.SetData(sizeBufferData);
        //Shader.SetGlobalBuffer("sizeBuffer", sizeBuffer);
        frustumCalc.SetBuffer("sizeBuffer", sizeBuffer);

        //更新buffer: indirect argument
        if (argsBuffer != null)
            argsBuffer.Release();
        uint meshIndicesNum = (uint)grassMesh.vertices.Length;
        uint[] args = new uint[5] { meshIndicesNum, 0, 0, 0, 0 };
        argsBuffer = new ComputeBuffer(1, sizeof(uint)*5,
            ComputeBufferType.IndirectArguments);
        argsBuffer.SetData(args);
        frustumCalc.SetBuffer("indirectDataBuffer", argsBuffer);

        //重新renderPosAppendBuffer
        if (renderPosAppendBuffer != null)
            renderPosAppendBuffer.Release();
        renderPosAppendBuffer = new ComputeBuffer(/*maxTileRenderCount*/16384,
            sizeof(float) * 3, ComputeBufferType.Append);
        renderPosAppendBuffer.SetCounterValue(0);
        frustumCalc.SetBuffer("renderPosAppend", renderPosAppendBuffer);
        Shader.SetGlobalBuffer("renderPosAppend", renderPosAppendBuffer);

        //更新counter
        if (counterBuffer != null)
            counterBuffer.Release();
        counterBuffer = new ComputeBuffer(1, sizeof(uint), ComputeBufferType.Counter);
        counterBuffer.SetCounterValue(0);
        frustumCalc.SetBuffer("counter", counterBuffer);



        frustumCalc.RunComputeShader();
        //render grass,TODO: LOD 64 32 16
        /*Graphics.DrawMeshInstancedIndirect(grassMesh,
            0, grassGen.grassMaterial, instanceBound, argsBuffer);*/

        //test
        /*uint[] argNum = { 0, 0, 0, 0, 0 };
        argsBuffer.GetData(argNum);
        //poses = new Vector2[argNum[1]];*/
        uint[] counter = { 0 };
        counterBuffer.GetData(counter);
        //poses = new Vector3[(int)(frustumSize.x * frustumSize.y)];
        poses = new Vector3[counter[0]];
        //renderPosAppendBuffer.GetData(poses);
        //Debug.Log(counter[0]);
        /*string str = "";
        for (int i = 0; i < poses.Length; i++) {
            str += (poses[i] + "  ");
        }
        Debug.Log(str);*/

        /*uint x, y, z;//test
        frustumCalcShader.GetKernelThreadGroupSizes(frustumKernel,out x, out y, out z);//8,8,1
        Debug.Log(x + " " + y + " " + z);*/
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(instanceBound.center, instanceBound.size);
        /*for(int i = 0; i < poses.Length; i++) {
            Gizmos.color = poses[i].y==1?Color.green:Color.black;
            Gizmos.DrawCube(new Vector3(poses[i].x, 50, poses[i].z)*2, Vector3.one);
        }*/
    }

    public void BuildTerrainTool() {
        terrainBuilder = new TerrainBuilder(heightMap, terrainHeight, terrainMat);
        GameObject terrain = GameObject.Find("terrain");
        if (terrain)
            DestroyImmediate(terrain);
        terrainBuilder.BuildTerrain(transform);
    }

    private void OnDisable() {
        grassGen.ReleaseBufer();
        terrainBuilder.ReleaseBuffer();
        argsBuffer.Release();
        sizeBuffer.Release();
        renderPosAppendBuffer.Release();
        counterBuffer.Release();
    }
}
