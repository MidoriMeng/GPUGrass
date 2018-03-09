using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TerrainBuilder))]
public class TerrainBuilderEditor : Editor {

    public override void OnInspectorGUI() {
        //base.OnInspectorGUI();
        //DrawDefaultInspector();
        TerrainBuilder script = (TerrainBuilder)target;

        //generate terrain
        EditorGUILayout.LabelField("Terrain Generation", EditorStyles.boldLabel);
        script.heightMap = (Texture2D)EditorGUILayout.ObjectField("Height Map", script.heightMap, typeof(Texture2D), false);
        script.terrainHeight = EditorGUILayout.FloatField("Terrain Height", script.terrainHeight);
        script.terrainScale = EditorGUILayout.FloatField("Terrain Scale", script.terrainScale);
        script.terrainMat= (Material)EditorGUILayout.ObjectField(
            "Terrain Material", script.terrainMat, typeof(Material), false);
        if (GUILayout.Button("Build Terrain")) {
            script.BuildTerrain();
        }
    }
}
