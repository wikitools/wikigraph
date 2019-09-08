using System;
using System.Reflection;
using UnityEditor;

namespace Inspector {
	public class InspectorUtils {
#if UNITY_EDITOR
		public static void DrawField(FieldInfo field, SerializedProperty fieldProperty, Object parent) {
			if (typeof(CustomInspectorProperty).IsAssignableFrom(field.FieldType)) {
				var mappingConfig = field.GetValue(parent);
				(mappingConfig as CustomInspectorProperty).DrawInInspector(fieldProperty);
			} else {
				EditorGUILayout.PropertyField(fieldProperty, true);
			}
		}

		public static void DrawObject(SerializedProperty objectProperty, Action drawContent) {
			EditorGUILayout.PropertyField(objectProperty, false);
			if (objectProperty.isExpanded) {
				EditorGUI.indentLevel++;
				drawContent();
				EditorGUI.indentLevel--;
			}
		}
#endif
	}
}