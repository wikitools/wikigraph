using System.Linq;
using InputModule.Event.Button;
using InputModule.Event.Interfaces;
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
				if (typeof(FlystickInput).IsAssignableFrom(GetType())) {
					EditorGUILayout.PropertyField(property.FindPropertyRelative("Instance"), false);
				}
				EditorGUILayout.PropertyField(property.FindPropertyRelative("IsButtonActivated"), false);
				if (IsButtonActivated) {
					var buttonProperty = property.FindPropertyRelative("Button");
					InspectorUtils.DrawField(GetType().GetField("Button"), buttonProperty, this);
				}
			});
		}
	}
}