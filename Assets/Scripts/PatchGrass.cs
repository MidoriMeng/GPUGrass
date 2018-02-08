using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//生成单块草地
public class PatchGrass : MonoBehaviour {
    public int patchSize = 4;
    public int grassAmountPerPatch = 64;
    public Material grassMaterial;

    private List<Vector3> positions;
    private List<Vector3> directions;
    private List<float> lengths;
    private List<float> densityIndexs;

    private List<Vector3> roots;

    private void Start() {
        GeneratePatch();
    }

    void GeneratePatch () {
        Vector3 startPosition = Vector3.zero;//待定
        //高度待定
        Vector3 curPos = startPosition;
        System.Random random = new System.Random();
        roots = new List<Vector3>();
        //随机生成位置
        int density = grassAmountPerPatch / patchSize / patchSize;
        for(int i = 0; i < patchSize; i++) {
            
            for(int j = 0; j < patchSize; j++) {
                Debug.Log(curPos);
                for(int k = 0; k < density; k++) {
                    roots.Add(new Vector3((float)(curPos.x + random.NextDouble()),
                        0, (float)(curPos.z + random.NextDouble())));
                }
                curPos += new Vector3(0, 0, 1);
            }
            curPos += new Vector3(1, 0, -4);
        }

        //生成草地
        Mesh m = new Mesh();
        m.vertices = roots.ToArray();
        int[] indices = new int[grassAmountPerPatch];
        for (int i = 0; i < indices.Length; i++)
            indices[i] = i;
        m.SetIndices(indices, MeshTopology.Points, 0);
        GameObject grassLayer = new GameObject("GrassLayer");
        grassLayer.transform.parent = gameObject.transform;
        MeshFilter filter = grassLayer.AddComponent<MeshFilter>();
        MeshRenderer renderer = grassLayer.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = grassMaterial;
        filter.mesh = m;

	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
