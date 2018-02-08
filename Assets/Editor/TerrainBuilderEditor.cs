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
        script.terrainSizeX = EditorGUILayout.IntField("Terrain Size X", script.terrainSizeX);
        script.terrainSizeZ = EditorGUILayout.IntField("Terrain Size X", script.terrainSizeZ);
        script.terrainMat= (Material)EditorGUILayout.ObjectField(
            "Terrain Material", script.terrainMat, typeof(Material), false);
        if (GUILayout.Button("Build Terrain")) {
            script.BuildTerrain();
        }

        EditorGUILayout.Separator();

        //grass options
        EditorGUILayout.LabelField("Grass Options", EditorStyles.boldLabel);
        script.grassDensityMap = (Texture2D)EditorGUILayout.ObjectField("Density Map", script.grassDensityMap, typeof(Texture2D), false);
        script.grassHeight = EditorGUILayout.FloatField("Grass Height", script.grassHeight);
        script.patchSize = EditorGUILayout.IntField("Patch Size", script.patchSize);
        script.grassAmountPerPatch = EditorGUILayout.IntField("Grass Amount Per Patch", script.grassAmountPerPatch);
        script.grassMaterial = (Material)EditorGUILayout.ObjectField(
            "Grass Material", script.grassMaterial, typeof(Material), false);
        if (GUILayout.Button("Build Terrain")) {
            script.RaiseGrass();
        }
    }
}
