using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrustumCalculation : MonoBehaviour {
    private TerrainBuilder tBuilder;

    //frustum calc
    public ComputeShader calcShader;
    private Vector2Int texSize = new Vector2Int();
    private Vector2Int threadGroupSize = new Vector2Int(2, 2);
    private int frustumKernel = 0;
    public GameObject plane;//for test
    private RenderTexture frustumTexture;//有用但不必放在这里，是为了测试
    ComputeBuffer renderPosAppendBuffer;//同上
    ComputeBuffer counterBuffer;

    private Bounds boundMin;//test


    /// <summary>
    /// 更新CS的RT(frustumTexture),
    /// grassMaterial的_FrustumStartPosI和instanceBound
    /// </summary>
    /// <param name="camera"></param>
    public Bounds UpdateComputeShader(Camera camera) {
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
        if (newTexSizeX != texSize.x || newTexSizeY != texSize.y) {
            //new testTexture texture
            //for test
            frustumTexture = new RenderTexture(newTexSizeX, newTexSizeY, 24);
            frustumTexture.enableRandomWrite = true;
            frustumTexture.Create();
            calcShader.SetTexture(frustumKernel, "testTexture", frustumTexture);
            plane.GetComponent<Renderer>().sharedMaterial.mainTexture = frustumTexture;
        }
        texSize.x = newTexSizeX; texSize.y = newTexSizeY;

        for (int i = 0; i < 3; i++) {
            Vector2Int t = tBuilder.GetTileIndex(frustum[i]);
            t -= tBuilder.GetTileIndex(camBound.min);
            frusIndex[i / 2] += new Vector4(t.x * Mathf.Abs(i - 1), t.y * Mathf.Abs(i - 1),
                t.x * (i % 2), t.y * (i % 2));
        }
        //Debug.Log(frusIndex[0].ToString() + "   " + frusIndex[1].ToString());
        calcShader.SetVectorArray(Shader.PropertyToID("frustumPosIndex"), frusIndex);
        Shader.SetGlobalVector(Shader.PropertyToID("threadSize"),
            new Vector4(texSize.x, texSize.y));
        var f = tBuilder.GetTileIndex(camBound.min);
        Shader.SetGlobalVector(Shader.PropertyToID("_FrustumStartPosI"),
            new Vector4(f.x, f.y));
        boundMin = camBound;//test
        return camBound;
    }


    public void RunComputeShader() {
        //运行shader  参数1=kid  参数2=线程组在x维度的数量 参数3=线程组在y维度的数量 参数4=线程组在z维度的数量
        calcShader.Dispatch(frustumKernel, texSize.x / threadGroupSize.x,
            texSize.y / threadGroupSize.y, 1);
        //Debug.Log(frustumTexSizeX / threadGroupSizeX + "   " +frustumTexSizeY / threadGroupSizeY);
    }

    
    // Use this for initialization
    void Start () {
        tBuilder = GameObject.Find("terrain").GetComponent<TerrainBuilder>();

        //setup
        frustumKernel = calcShader.FindKernel("FrustumCulling");
        renderPosAppendBuffer = new ComputeBuffer(1, sizeof(float) * 4, ComputeBufferType.Append);
        renderPosAppendBuffer.SetCounterValue(0);
        counterBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Counter);
        counterBuffer.SetCounterValue(0);
        calcShader.SetBuffer(frustumKernel, "renderPosAppend", renderPosAppendBuffer);
        calcShader.SetBuffer(frustumKernel, "counter", counterBuffer);
        //test
        plane.GetComponent<Renderer>().sharedMaterial.mainTexture = frustumTexture;
    }
	
	// Update is called once per frame
	void Update () {
        //test
        int[] argNum = { 0 };
        counterBuffer.GetData(argNum);
        Vector4[] poses = new Vector4[argNum[0]];
        renderPosAppendBuffer.GetData(poses);
        string str = "";
        for (int i = 0; i < Mathf.Min(20, argNum[0]); i++) {
            str += (poses[i] + "  ");
        }
        Debug.Log(str);
        //test plane
        int patchSize = TerrainBuilder.PATCH_SIZE;
        plane.transform.localScale =
            new Vector3(patchSize * texSize.x,
            patchSize * texSize.y, 1);
        plane.transform.position = boundMin.min +
            new Vector3(patchSize * texSize.x / 2,
            0, patchSize * texSize.y / 2);

        counterBuffer.SetCounterValue(0);//应该放到run中
    }


}
