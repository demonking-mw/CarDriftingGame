using UnityEngine;
using UnityEditor;
using System.IO;

public class TrackLoaderEditor : EditorWindow
{
    public GameObject tirePrefab;
    public string fileName = "trackData1.csv";

    [MenuItem("Tools/Load Track")]
    public static void ShowWindow()
    {
        GetWindow<TrackLoaderEditor>("Load Track");
    }

    void OnGUI()
    {
        tirePrefab = (GameObject)EditorGUILayout.ObjectField("Tire Prefab", tirePrefab, typeof(GameObject), false);
        fileName = EditorGUILayout.TextField("File Name", fileName);

        if (GUILayout.Button("Load Track"))
        {
            LoadTrackData();
        }
    }

    void LoadTrackData()
    {
        string path = Path.Combine(Application.dataPath, fileName);

        if (!File.Exists(path))
        {
            Debug.LogError($"File not found: {path}");
            return;
        }

        string[] lines = File.ReadAllLines(path);

        foreach (string line in lines)
        {
            string[] values = line.Split(',');

            if (values.Length == 2 && 
                float.TryParse(values[0], out float x) && 
                float.TryParse(values[1], out float y))
            {
                Vector2 position = new Vector2(x, y);
                Instantiate(tirePrefab, position, Quaternion.identity);
            }
        }

        Debug.Log("Track loaded successfully.");
    }
}
