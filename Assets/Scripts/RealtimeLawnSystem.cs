using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RealtimeLawnSystem : MonoBehaviour {
    public GrassGenerator grassGen;
    public TerrainBuilder terrain;
    public FrustumCalculation frustumCalc;
    private Mesh grassMesh;

    //indirect
    private ComputeBuffer argsBuffer;
    private Bounds instanceBound;
    // Use this for initialization
    void Start() {
        //草叶
        grassMesh = grassGen.generateGrassTile();
        grassGen.PregenerateGrassInfo();
        //地形
        terrain.BuildTerrain();
        terrain.BuildTerrainDataBuffer();

        //draw indirect arguments
        uint meshIndicesNum = (uint)grassMesh.vertices.Length;//grassMesh.GetIndexCount(0);
        uint[] args = new uint[5] { meshIndicesNum, 0, 0, 0, 0 };
        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint),
            ComputeBufferType.IndirectArguments);
        //frustumCalcShader.SetBuffer(frustumKernel, "indirectDataBuffer", argsBuffer);
        Shader.SetGlobalBuffer("indirectDataBuffer", argsBuffer);
    }

    // Update is called once per frame
    void Update() {
        //视锥体计算
        instanceBound = frustumCalc.UpdateComputeShader(Camera.main);
        frustumCalc.RunComputeShader();

        //render grass,TODO: LOD 64 32 16
        Graphics.DrawMeshInstancedIndirect(grassMesh,
            0, grassGen.grassMaterial, instanceBound, argsBuffer);

        /*uint x, y, z;//test
        frustumCalcShader.GetKernelThreadGroupSizes(frustumKernel,out x, out y, out z);//8,8,1
        Debug.Log(x + " " + y + " " + z);*/
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(instanceBound.center, instanceBound.size);
    }
}
