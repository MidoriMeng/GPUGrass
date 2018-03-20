using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GrassBuilder))]
public class GrassBuilderEditor : Editor {

    public override void OnInspectorGUI() {
        //base.OnInspectorGUI();
        //DrawDefaultInspector();
        GrassBuilder script = (GrassBuilder)target;
        
        script.grassDensityMap = (Texture2D)EditorGUILayout.ObjectField("Density Map", script.grassDensityMap, typeof(Texture2D), false);
        script.grassHeightMax = EditorGUILayout.FloatField("Grass Height", script.grassHeightMax);
        script.pregenerateGrassAmount = EditorGUILayout.IntField("pregenerate grass amount", script.pregenerateGrassAmount);
        script.grassAmountPerTile = EditorGUILayout.IntField("Grass Amount Per Tile", script.grassAmountPerTile);
        script.grassMaterial = (Material)EditorGUILayout.ObjectField(
            "Grass Material", script.grassMaterial, typeof(Material), false);
        script.grassMesh = (Mesh)EditorGUILayout.ObjectField(
            "Grass Mesh", script.grassMesh, typeof(Mesh), false);
        if (GUILayout.Button("Build Grass")) {
            script.PregenerateGrassInfo();
            script.calculateTileToRender();
        }
    }
}
