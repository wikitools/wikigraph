using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Linq;

[CustomEditor(typeof(SwitchScene_PrepareExample))]
public class SwitchScene_PrepareExampleEditor : Editor
{
    public override void OnInspectorGUI()
    {
        GUILayout.Label("Switch scene example", new GUIStyle("BoldLabel") { fontSize = 18, padding = new RectOffset(0, 0, 10, 10), alignment = TextAnchor.MiddleCenter });

        if (GUILayout.Button(new GUIContent(" MANUAL", EditorGUIUtility.IconContent("_Help").image), GUILayout.Height(30)))
            Application.OpenURL(Lzwp.LzwpLibManualUrl + "#switch-scene-example");

        GUILayout.Space(20);

        GUIStyle helpBoxStyle = new GUIStyle("HelpBox") { fontSize = 16, padding = new RectOffset(30,30,30,30) };
        GUILayout.Label("In order for the scenes to be loaded, they must be added to the build settings.\nClick 'Prepare example' button to add two example scenes to that list; later click 'Clear example' to remove them from it.", helpBoxStyle);

        GUILayout.Space(15);
        GUILayout.BeginHorizontal();

        GUI.enabled = CountExampleScenesOnList() < 2;

        if (GUILayout.Button("Prepare example", GUILayout.Height(30)))
        {
            RemoveExampleScenes();

            List<EditorBuildSettingsScene> newScenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);

            newScenes.Add(new EditorBuildSettingsScene(GetScene1Path(), true));
            newScenes.Add(new EditorBuildSettingsScene(GetScene2Path(), true));

            EditorBuildSettings.scenes = newScenes.ToArray();
        }

        GUI.enabled = CountExampleScenesOnList() > 0;

        if (GUILayout.Button("Clear example", GUILayout.Height(30)))
        {
            RemoveExampleScenes();
        }

        GUI.enabled = true;

        GUILayout.EndHorizontal();
        GUILayout.Space(20);
    }

    int CountExampleScenesOnList()
    {
        return EditorBuildSettings.scenes.Where(s => s.path == GetScene1Path() || s.path == GetScene2Path()).Count();
    }

    void RemoveExampleScenes()
    {
        List<EditorBuildSettingsScene> newScenes = new List<EditorBuildSettingsScene>();

        foreach (var scene in EditorBuildSettings.scenes)
            if (scene.path != GetScene1Path() && scene.path != GetScene2Path())
                newScenes.Add(scene);

        EditorBuildSettings.scenes = newScenes.ToArray();
    }

    string GetScene1Path()
    {
        return GetThisExampleDir() + "/" + SwitchSceneExample.SCENE_1_NAME + ".unity";
    }

    string GetScene2Path()
    {
        return GetThisExampleDir() + "/" + SwitchSceneExample.SCENE_2_NAME + ".unity";
    }

    string GetThisExampleDir()
    {
        return Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this)))));
    }
}
