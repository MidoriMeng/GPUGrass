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

        EditorGUILayout.Separator();

        //grass options
        EditorGUILayout.LabelField("Grass Options", EditorStyles.boldLabel);
        script.grassDensityMap = (Texture2D)EditorGUILayout.ObjectField("Density Map", script.grassDensityMap, typeof(Texture2D), false);
        script.grassHeightMax = EditorGUILayout.FloatField("Grass Height", script.grassHeightMax);
        script.patchSize = EditorGUILayout.IntField("Patch Size", script.patchSize);
        script.grassAmountPerTile = EditorGUILayout.IntField("Grass Amount Per Tile", script.grassAmountPerTile);
        script.grassMaterial = (Material)EditorGUILayout.ObjectField(
            "Grass Material", script.grassMaterial, typeof(Material), false);
        if (GUILayout.Button("Build Grass")) {
            script.PregenerateGrass();
            script.calculateTileToRender();
        }
    }
}
