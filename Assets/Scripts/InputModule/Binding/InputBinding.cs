using System;
using System.Reflection;
using InputModule.Event.Interfaces;
using Inspector;
using UnityEditor;

namespace InputModule.Binding {
	public abstract class InputBinding : CustomInspectorProperty, InputPoller, InputInitializer {
#if UNITY_EDITOR
		public void DrawInInspector(SerializedProperty property) {
			InspectorUtils.DrawObject(property, () => {
				foreach (var field in GetType().GetFields()) {
					var fieldProperty = property.FindPropertyRelative(field.Name);
					InspectorUtils.DrawField(field, fieldProperty, this);
				}
			});
		}
#endif

		public void CheckForInput() {
			CallFieldsOfType<InputPoller>(field => field.CheckForInput());
		}

		public void Init() {
			CallFieldsOfType<InputInitializer>(field => field.Init());
		}

		public void CallFieldsOfType<F>(Action<F> function) where F : class => CallFieldsOfType(function, f => true);

		public void CallFieldsOfType<F>(Action<F> function, Func<FieldInfo, bool> condition) where F : class {
			foreach (var field in GetType().GetFields()) {
				if (typeof(F).IsAssignableFrom(field.FieldType) && condition(field))
					function(field.GetValue(this) as F);
			}
		}
	}
}