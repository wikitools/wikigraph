using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Tracking_LzwpExampleReadme))]
public class Tracking_LzwpExampleReadmeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        GUILayout.Label("Tracking example", new GUIStyle("BoldLabel") { fontSize = 18, padding = new RectOffset(0, 0, 10, 10), alignment = TextAnchor.MiddleCenter });

        if (GUILayout.Button(new GUIContent(" MANUAL", EditorGUIUtility.IconContent("_Help").image), GUILayout.Height(30)))
            Application.OpenURL(Lzwp.LzwpLibManualUrl + "#tracking-example");

        GUILayout.Space(20);
    }
}
