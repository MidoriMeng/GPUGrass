using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RealtimeLawnSystem : MonoBehaviour {
    public GrassGenerator grassGenerator;
    public FrustumCalculation frustumCalc;
    private Mesh grassMesh;

    //indirect
    private ComputeBuffer argsBuffer;
    private Bounds instanceBound;
    // Use this for initialization
    void Start () {
        grassMesh = grassGenerator.generateGrassTile();

        //draw indirect arguments
        uint meshIndicesNum = (uint)grassMesh.vertices.Length;//grassMesh.GetIndexCount(0);
        uint[] args = new uint[5] { meshIndicesNum, 0, 0, 0, 0 };
        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint),
            ComputeBufferType.IndirectArguments);
        //frustumCalcShader.SetBuffer(frustumKernel, "indirectDataBuffer", argsBuffer);
        Shader.SetGlobalBuffer("indirectDataBuffer", argsBuffer);
    }

    // Update is called once per frame
    void Update () {
        //视锥体计算
        instanceBound = frustumCalc.UpdateFrustumComputeShader(Camera.main);
        frustumCalc.RunComputeShader();

        //render grass,TODO: LOD 64 32 16
        Graphics.DrawMeshInstancedIndirect(grassMesh,
            0, grassGenerator.grassMaterial, instanceBound, argsBuffer);
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(instanceBound.center, instanceBound.size);
    }
}
