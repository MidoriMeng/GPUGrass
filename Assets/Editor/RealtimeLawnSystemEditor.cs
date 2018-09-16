using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RealtimeLawnSystem))]
public class RealtimeLawnSystemEditor : Editor {
    public override void OnInspectorGUI() {
        RealtimeLawnSystem script = (RealtimeLawnSystem)target;

        //terrain
        EditorGUILayout.LabelField("Terrain Generation", EditorStyles.boldLabel);
        script.heightMap = (Texture2D)EditorGUILayout.ObjectField("Height Map", script.heightMap, typeof(Texture2D), false);
        script.terrainHeight = EditorGUILayout.FloatField(
            "Terrain Height", script.terrainHeight);
        script.terrainMat = (Material)EditorGUILayout.ObjectField(
            "Terrain Material", script.terrainMat, typeof(Material), false);
        if (GUILayout.Button("Build Terrain")) {
            script.BuildTerrainTool();
        }

        //calc shader
        EditorGUILayout.LabelField("Frustum Calculation", EditorStyles.boldLabel);
        script.calcShader = (ComputeShader)EditorGUILayout.ObjectField(
            "Calculation Shader", script.calcShader, typeof(ComputeShader), false);

        //grass
        EditorGUILayout.LabelField("Grass Generation", EditorStyles.boldLabel);
        script.grassDensityMap = (Texture2D)EditorGUILayout.ObjectField(
            "Grass Density Map", script.grassDensityMap, typeof(Texture2D), false);
        script.grassMaterial = (Material)EditorGUILayout.ObjectField(
            "Grass Material", script.grassMaterial, typeof(Material), false);
        /*script.grassAmountPerTile = EditorGUILayout.IntField(
            "Grass Amount Per Tile (max)", script.grassAmountPerTile);*/
        script.minGrassPerTile = EditorGUILayout.IntField(
            "Grass Amount Per Tile (min)", script.minGrassPerTile);
        script.pregenerateGrassAmount = EditorGUILayout.IntField(
            "Pregenerate Grass Patch Length", script.pregenerateGrassAmount);
        script.bladeSectionCountMax = EditorGUILayout.IntField(
            "Blade Section Count (Max)", script.bladeSectionCountMax);
        script.bladeSectionCountMin = EditorGUILayout.IntField(
            "Blade Section Count (Min)", script.bladeSectionCountMin);
    }
}
