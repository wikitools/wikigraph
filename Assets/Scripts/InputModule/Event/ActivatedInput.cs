using InputModule.Event.Button;
using InputModule.Event.Cursor;
using InputModule.Event.Interfaces;
using InputModule.Event.Pointer;
using Inspector;
using UnityEditor;

namespace InputModule.Event {
	public abstract class ActivatedInput: InputInitializer, InputPoller, CustomInspectorProperty {
		public bool IsButtonActivated;

		private ButtonInput cachedButton;
		protected ButtonInput ActivationButton => cachedButton ?? (cachedButton = (ButtonInput) GetType().GetField("Button").GetValue(this));

		public abstract void Init();

		public void CheckForInput() {
			if (IsButtonActivated && typeof(InputPoller).IsAssignableFrom(ActivationButton.GetType()))
				(ActivationButton as InputPoller).CheckForInput();
		}
		
		public void DrawInInspector(SerializedProperty property) {
			InspectorUtils.DrawObject(property, () => {
				EditorGUILayout.PropertyField(property.FindPropertyRelative("IsButtonActivated"), false);
				if(typeof(CursorInput).IsAssignableFrom(GetType()))
					EditorGUILayout.PropertyField(property.FindPropertyRelative("MainAxisOnly"), false);
				if (typeof(FlystickInput).IsAssignableFrom(GetType()))
					EditorGUILayout.PropertyField(property.FindPropertyRelative("Instance"), false);
				if (IsButtonActivated && typeof(PointerInput).IsAssignableFrom(GetType()))
					EditorGUILayout.PropertyField(property.FindPropertyRelative("ActivationType"), false);
				if (IsButtonActivated) {
					var buttonProperty = property.FindPropertyRelative("Button");
					InspectorUtils.DrawField(GetType().GetField("Button"), buttonProperty, this);
				}
			});
		}
	}
}