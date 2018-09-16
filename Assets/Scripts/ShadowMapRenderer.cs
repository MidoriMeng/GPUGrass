using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* *emmm还没开发完*/
public class ShadowMapRenderer : MonoBehaviour {
    RenderTexture shadowDepthTexture;
    int texResolution = 2048;
    Camera shadowCam;

    private Camera CreateShadowCamera(string name) {
        var go = GameObject.Find(name);
        if (!go) {
            go = new GameObject(name);
        }

        Camera goCamera = go.GetComponent<Camera>();
        if (!goCamera) {
            goCamera = go.AddComponent<Camera>();
        }

        go.hideFlags = HideFlags.HideAndDontSave;
        goCamera.enabled = false;
        goCamera.renderingPath = RenderingPath.Forward;
        goCamera.nearClipPlane = 0.1f;
        goCamera.farClipPlane = 100.0f;
        goCamera.depthTextureMode = DepthTextureMode.None;
        goCamera.clearFlags = CameraClearFlags.Depth;
        goCamera.backgroundColor = Color.white;
        goCamera.orthographic = false;
        goCamera.hideFlags = HideFlags.HideAndDontSave;
        go.SetActive(false);
        return goCamera;
    }

    void Start () {
        shadowDepthTexture = new RenderTexture(texResolution, texResolution,
            24, RenderTextureFormat.Shadowmap, RenderTextureReadWrite.Linear);
        shadowCam = CreateShadowCamera("Shadow Map Camera");
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void RenderShadowMap(Camera shadowCamera) {
        Matrix4x4 vp = shadowCamera.projectionMatrix * shadowCamera.worldToCameraMatrix;
    }
}
