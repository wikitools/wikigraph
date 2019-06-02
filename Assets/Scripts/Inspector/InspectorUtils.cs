using System;
using System.Reflection;
using UnityEditor;

namespace Inspector {
	public class InspectorUtils {
		public static void DrawField(FieldInfo field, SerializedProperty fieldProperty, Object parent) {
			if (typeof(CustomInspectorProperty).IsAssignableFrom(field.FieldType)) {
				var mappingConfig = field.GetValue(parent);
				(mappingConfig as CustomInspectorProperty).DrawInInspector(fieldProperty);
			} else {
				EditorGUILayout.PropertyField(fieldProperty, true);
			}
		}
	}
}