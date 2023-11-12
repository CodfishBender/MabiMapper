using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GenerateTerrain))]
public class GenerateTerrainEditor : Editor
{
    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        GenerateTerrain myScript = (GenerateTerrain)target;
        GUILayoutOption guiLayoutHeight = GUILayout.Height(30);

        GUILayout.Space(20);
        if (GUILayout.Button("Select Region File", guiLayoutHeight)) {
            myScript.OpenRegionFile();
        }
        if (GUILayout.Button("Select Data Folder", guiLayoutHeight)) {
            myScript.OpenDataFolder();
        }
        GUILayout.Space(20);
        if (GUILayout.Button("Generate Terrain", guiLayoutHeight)) {
            myScript.CreateTerrain();
        }
        if (GUILayout.Button("Clear Terrain", guiLayoutHeight)) {
            myScript.ClearTerrain();
        }
        GUILayout.Space(20);
        if (GUILayout.Button("Spawn Props", guiLayoutHeight)) {
            myScript.SpawnProps();
        }
        if (GUILayout.Button("Clear Props", guiLayoutHeight)) {
            myScript.ClearProps();
        }
    }
}
