#if UNITY_3_0||UNITY_3_1||UNITY_3_2||UNITY_3_3||UNITY_3_4||UNITY_3_5||UNITY_3_6||UNITY_3_7||UNITY_3_8||UNITY_3_9
#define UNITY_3
#endif

using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

[CustomEditor(typeof(TOD_Sky))]
public class TOD_SkyInspector : Editor
{
	public override void OnInspectorGUI()
	{
		#if UNITY_3
		EditorGUIUtility.LookLikeInspector();
		#endif

		GUILayout.BeginHorizontal();
		if (GUILayout.Button("Import"))
		{
			var folder = EditorPrefs.GetString("Time of Day Folder", Application.dataPath);

			var sky  = Selection.activeGameObject.GetComponent<TOD_Sky>();
			var path = EditorUtility.OpenFilePanel("Import", folder, "xml");

			if (string.IsNullOrEmpty(path)) return;

			var serializer = new XmlSerializer(typeof(TOD_Parameters));

			using (var filestream = new FileStream(path, FileMode.Open))
			{
				var reader = new XmlTextReader(filestream);
				var parameters = serializer.Deserialize(reader) as TOD_Parameters;
				parameters.ToSky(sky);
				EditorUtility.SetDirty(sky);
			}

			EditorPrefs.SetString("Time of Day Folder", Path.GetDirectoryName(path));
			EditorPrefs.SetString("Time of Day File", Path.GetFileName(path));
		}
		if (GUILayout.Button("Export"))
		{
			var folder = EditorPrefs.GetString("Time of Day Folder", Application.dataPath);
			var file   = EditorPrefs.GetString("Time of Day File", "Time of Day.xml");

			var sky  = Selection.activeGameObject.GetComponent<TOD_Sky>();
			var path = EditorUtility.SaveFilePanel("Export", folder, file, "xml");

			if (string.IsNullOrEmpty(path)) return;

			var serializer = new XmlSerializer(typeof(TOD_Parameters));

			using (var filestream = new FileStream(path, FileMode.Create))
			{
				var parameters = new TOD_Parameters(sky);
				var writer = new XmlTextWriter(filestream, Encoding.Unicode);
				serializer.Serialize(writer, parameters);
				AssetDatabase.Refresh();
			}

			EditorPrefs.SetString("Time of Day Folder", Path.GetDirectoryName(path));
			EditorPrefs.SetString("Time of Day File", Path.GetFileName(path));
		}
		GUILayout.EndHorizontal();

		DrawDefaultInspector();
	}
}
