using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TerrainBuilder))]
public class TerrainBuilderEditor : Editor {

    public override void OnInspectorGUI() {
        //base.OnInspectorGUI();
        DrawDefaultInspector();
        TerrainBuilder script = (TerrainBuilder)target;
        if(GUILayout.Button("Build Terrain")) {
            script.BuildTerrain();
        }
    }
}
