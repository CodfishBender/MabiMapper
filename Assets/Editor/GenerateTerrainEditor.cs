using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GenerateTerrain))]
public class GenerateTerrainEditor : Editor
{
    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        GenerateTerrain myScript = (GenerateTerrain)target;
        GUILayoutOption guiLayoutHeight = GUILayout.Height(30);

        if (GUILayout.Button("Open Region File", guiLayoutHeight)) {
            myScript.OpenRegionFile();
        }
        if (GUILayout.Button("Select Data Folder", guiLayoutHeight)) {
            myScript.OpenDataFolder();
        }
        if (GUILayout.Button("Generate Terrain", guiLayoutHeight)) {
            myScript.CreateTerrain();
        }
        if (GUILayout.Button("Clear Terrain", guiLayoutHeight)) {
            myScript.ClearTerrain();
        }
    }
}
