using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GenerateTerrain))]
public class GenerateTerrainEditor : Editor
{
    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        GenerateTerrain script = (GenerateTerrain)target;
        GUILayoutOption guiLayoutHeight = GUILayout.Height(30);
        GUILayoutOption propGuiWidth = GUILayout.Width(150);

        GUILayout.Space(20);
        GUILayout.Label("Select Files", EditorStyles.whiteLargeLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Region File", guiLayoutHeight)) script.OpenRegionDialog();
        if (GUILayout.Button("Data Folder", guiLayoutHeight)) script.OpenDataDialog();
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(20);
        GUILayout.Label("Terrain", EditorStyles.whiteLargeLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Generate", guiLayoutHeight)) script.CreateTerrain();
        if (GUILayout.Button("Clear", guiLayoutHeight)) script.ClearTerrain();
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(20);
        GUILayout.Label("Props", EditorStyles.whiteLargeLabel);
        if (GUILayout.Button("All Props", guiLayoutHeight)) script.SpawnProps(true, true, true);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Normal", guiLayoutHeight)) script.SpawnProps(true, false, false);
        if (GUILayout.Button("Event", guiLayoutHeight)) script.SpawnProps(false, true, false);
        if (GUILayout.Button("Disabled", guiLayoutHeight)) script.SpawnProps(false, false, true);
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(10);
        if (GUILayout.Button("Clear", guiLayoutHeight)) script.ClearProps();
    }
}
