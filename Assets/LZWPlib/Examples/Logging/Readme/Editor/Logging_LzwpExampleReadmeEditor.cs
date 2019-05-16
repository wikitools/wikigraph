using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Logging_LzwpExampleReadme))]
public class Logging_LzwpExampleReadmeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        GUILayout.Label("Logging example", new GUIStyle("BoldLabel") { fontSize = 18, padding = new RectOffset(0, 0, 10, 10), alignment = TextAnchor.MiddleCenter });

        if (GUILayout.Button(new GUIContent(" MANUAL", EditorGUIUtility.IconContent("_Help").image), GUILayout.Height(30)))
            Application.OpenURL(Lzwp.LzwpLibManualUrl + "#logging-example");

        GUILayout.Space(20);
    }
}
