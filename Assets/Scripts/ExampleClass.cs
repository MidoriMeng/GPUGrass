using UnityEngine;
using System.Collections;

public class ExampleClass : MonoBehaviour {
    public RealtimeLawnSystem lawn;
    public int instanceCount = 500;
    public Mesh grassMesh;
    public Material instanceMaterial;

    private int cachedInstanceCount = -1;
    private ComputeBuffer positionBuffer;
    private ComputeBuffer argsBuffer;
    private uint[] args = new uint[5] { 0, 0, 0, 0, 0 };

    void Start() {
        grassMesh = lawn.grassMesh;
        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        UpdateBuffers();
    }

    void Update() {
        // Update starting position buffer
        if (cachedInstanceCount != instanceCount)
            UpdateBuffers();

        // Render
        Graphics.DrawMeshInstancedIndirect(grassMesh, 0, instanceMaterial, new Bounds(Vector3.zero, new Vector3(100.0f, 100.0f, 100.0f)), argsBuffer);
    }

    void UpdateBuffers() {

        // positions
        if (positionBuffer != null)
            positionBuffer.Release();
        positionBuffer = new ComputeBuffer(instanceCount, 16);
        Vector4[] positions = new Vector4[instanceCount];
        positionBuffer.SetData(positions);
        instanceMaterial.SetBuffer("positionBuffer", positionBuffer);

        // indirect args
        uint numIndices = (grassMesh != null) ? (uint)grassMesh.GetIndexCount(0) : 0;
        //Debug.Log(numIndices + "  " + grassMesh.vertices.Length);
        args[0] = numIndices;
        args[1] = (uint)instanceCount;
        argsBuffer.SetData(args);

        cachedInstanceCount = instanceCount;
    }

    void OnDisable() {

        if (positionBuffer != null)
            positionBuffer.Release();
        positionBuffer = null;

        if (argsBuffer != null)
            argsBuffer.Release();
        argsBuffer = null;
    }
}