using UnityEditor;
using UnityEngine;
using LZWPlibEditor.Editors;
using Newtonsoft.Json.Linq;

[CustomEditor(typeof(EditorOnly_EditConfigInPlayMode))]
public class EditorOnly_EditConfigInPlayModeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        if (Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Sometimes changing a value may not cause any effect, e.g. when that value has been cached or used only at startup.", MessageType.Info);
            this.DrawDefaultInspectorWithoutScriptField();

            EditorOnly_EditConfigInPlayMode c = target as EditorOnly_EditConfigInPlayMode;

            if (c.applyCustomConfigChanges)
                Lzwp.config.SetCustom(JToken.FromObject(c.customConfig));
        }
        else
            EditorGUILayout.HelpBox("Enter Play mode to view and edit loaded config.\nSometimes changing a value may not cause any effect, e.g. when that value has been cached or used only at startup.", MessageType.Info);
    }
}
