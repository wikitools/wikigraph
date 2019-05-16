using System.IO;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CustomConfig_PrepareExample))]
public class CustomConfig_PrepareExampleEditor : Editor
{
    const string EXAMPLE_SCRIPT_FILENAME = "CustomConfigUsage_LzwpExample";

    public override void OnInspectorGUI()
    {
        GUILayout.Label("Custom config example", new GUIStyle("BoldLabel") { fontSize = 18, padding = new RectOffset(0, 0, 10, 10), alignment = TextAnchor.MiddleCenter });

        if (GUILayout.Button(new GUIContent(" MANUAL", EditorGUIUtility.IconContent("_Help").image), GUILayout.Height(30)))
            Application.OpenURL(Lzwp.LzwpLibManualUrl + "#config-example");

        GUILayout.Space(20);

        GUIStyle helpBoxStyle = new GUIStyle("HelpBox") { fontSize = 16, padding = new RectOffset(30, 30, 30, 30) };
        GUILayout.Label("For this example first you need to generate script " + EXAMPLE_SCRIPT_FILENAME + ".cs\n\nClick 'Prepare example' button to create this file; later click 'Clear example' to remove it.", helpBoxStyle);

        GUILayout.Space(15);
        GUILayout.BeginHorizontal();

        bool scriptFileExists = File.Exists(GetDestFilePath());

        GUI.enabled = !scriptFileExists;

        if (GUILayout.Button("Prepare example", GUILayout.Height(30)))
            GenerateFile();

        GUI.enabled = scriptFileExists;

        if (GUILayout.Button("Clear example", GUILayout.Height(30)))
            RemoveFile();

        GUI.enabled = true;

        GUILayout.EndHorizontal();

        GUILayout.Space(20);
    }

    void GenerateFile()
    {
        if (!File.Exists(GetTemplateFilePath()))
        {
            EditorUtility.DisplayDialog("Prepare custom config example", string.Format("Cannot find example script template file :(\n\n(file: {0}", GetTemplateFilePath()), "OK");
            return;
        }

        if (!File.Exists(GetDestFilePath()) || EditorUtility.DisplayDialog("Prepare custom config example", "Example script " + EXAMPLE_SCRIPT_FILENAME + ".cs already exists in example directory.\nOwerwrite?", "Yes", "No"))
        {
            File.Copy(GetTemplateFilePath(), GetDestFilePath(), true);
            AssetDatabase.Refresh();
        }
    }

    void RemoveFile()
    {
        if (File.Exists(GetDestFilePath()))
        {
            if (File.ReadAllText(GetTemplateFilePath()) == File.ReadAllText(GetDestFilePath()) || EditorUtility.DisplayDialog("Clear custom config example", "Example script (" + EXAMPLE_SCRIPT_FILENAME + ".cs) has been modified.\nAre you sure you want to delete this file?", "Yes", "No"))
            {
                File.Delete(GetDestFilePath());
                AssetDatabase.Refresh();
            }
        }
    }

    string GetTemplateFilePath()
    {
        return GetThisScriptDir() + "/" + EXAMPLE_SCRIPT_FILENAME + ".txt";
    }

    string GetDestFilePath()
    {
        return GetThisExampleDir() + "/" + EXAMPLE_SCRIPT_FILENAME + ".cs";
    }

    string GetThisExampleDir()
    {
        return Path.GetDirectoryName(Path.GetDirectoryName(GetThisScriptDir()));
    }

    string GetThisScriptDir()
    {
        return Path.GetDirectoryName(AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this)));
    }
}
