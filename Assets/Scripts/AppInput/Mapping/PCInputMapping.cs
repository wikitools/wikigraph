using AppInput.Event.Button;
using Inspector;
using UnityEditor;

namespace AppInput.Mapping {
	[System.Serializable]
	public class PCInputMapping: CustomInspectorProperty, InputMapping {
		public PCButtonEvent RotationButton;
		
		public void DrawInInspector(SerializedProperty property) {
			EditorGUILayout.PropertyField(property, false);
			if (!property.isExpanded) return;
			EditorGUI.indentLevel++;
			foreach (var field in GetType().GetFields()) {
				var fieldProperty = property.FindPropertyRelative(field.Name);
				InspectorUtils.DrawField(field, fieldProperty, this);
			}
		}
	}
}