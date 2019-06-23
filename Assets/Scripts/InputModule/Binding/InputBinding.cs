using System;
using InputModule.Event.Interfaces;
using Inspector;
using UnityEditor;
using UnityEngine;

namespace InputModule.Binding {
	public abstract class InputBinding: CustomInspectorProperty, InputPoller, InputInitializer {
		public void DrawInInspector(SerializedProperty property) {
			InspectorUtils.DrawObject(property, () => {
				foreach (var field in GetType().GetFields()) {
					var fieldProperty = property.FindPropertyRelative(field.Name);
					InspectorUtils.DrawField(field, fieldProperty, this);
				}
			});
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
					function(field.GetValue(this) as T);
			}
		}
	}
}