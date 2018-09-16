#if UNITY_3_0||UNITY_3_1||UNITY_3_2||UNITY_3_3||UNITY_3_4||UNITY_3_5||UNITY_3_6||UNITY_3_7||UNITY_3_8||UNITY_3_9
#define UNITY_3
#endif

using UnityEngine;
using UnityEditor;
using System.IO;

[CustomEditor(typeof(TOD_Camera))]
public class TOD_CameraInspector : Editor
{
	public override void OnInspectorGUI()
	{
		#if UNITY_3
		EditorGUIUtility.LookLikeInspector();
		#endif

		GUILayout.BeginHorizontal();
		if (GUILayout.Button("Update Sky Dome Position"))
		{
			(target as TOD_Camera).DoDomePosToCamera();
		}
		if (GUILayout.Button("Update Sky Dome Scale"))
		{
			(target as TOD_Camera).DoDomeScaleToFarClip();
		}
		GUILayout.EndHorizontal();

		DrawDefaultInspector();
	}
}
