using System;
using AppInput.Event.Interfaces;
using Inspector;
using UnityEditor;

namespace AppInput.Mapping {
	public abstract class InputMapping: CustomInspectorProperty, InputPoller, InputInitializer {
		public void DrawInInspector(SerializedProperty property) {
			EditorGUILayout.PropertyField(property, false);
			if (!property.isExpanded) return;
			EditorGUI.indentLevel++;
			foreach (var field in GetType().GetFields()) {
				var fieldProperty = property.FindPropertyRelative(field.Name);
				InspectorUtils.DrawField(field, fieldProperty, this);
			}
			EditorGUI.indentLevel--;
		}

		public void CheckForInput() {
			CallFieldsOfType<InputPoller>(field => field.CheckForInput());
		}

		public void Init() {
			CallFieldsOfType<InputInitializer>(field => field.Init());
		}

		private void CallFieldsOfType<T>(Action<T> function) where T: class {
			foreach (var field in GetType().GetFields()) {
				if (typeof(T).IsAssignableFrom(field.FieldType))
					function(field as T);
			}
		}
	}
}