using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstancingTest : MonoBehaviour {
    public Transform prefab;
    public int instances = 50;

    public float radius = 20f;

    // Use this for initialization
    void Start () {
        MaterialPropertyBlock properties = new MaterialPropertyBlock();
        for (int i = 0; i < instances; i++) {
            Transform t = Instantiate(prefab);
            t.localPosition = Random.insideUnitSphere * radius;
            t.SetParent(transform);

            properties.SetColor(
                "_Color", new Color(Random.value, Random.value, Random.value)
            );
            t.GetComponent<MeshRenderer>().SetPropertyBlock(properties);
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
