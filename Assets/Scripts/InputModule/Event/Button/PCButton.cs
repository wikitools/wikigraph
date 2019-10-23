using System;
using InputModule.Event.Interfaces;
using Inspector;
using UnityEngine;
using UnityEditor;

namespace InputModule.Event.Button {
	[Serializable]
	public class PCButton : ButtonInput, CustomInspectorProperty, InputPoller {
		public ButtonType ButtonType;
		public MouseButton MouseButton;
		public KeyCode KeyboardButton;

		public void CheckForInput() {
			if (ButtonType == ButtonType.Mouse ? Input.GetMouseButtonDown((int) MouseButton) : Input.GetKeyDown(KeyboardButton)) {
				Pressed = true;
				OnPress?.Invoke();
			}
			if (ButtonType == ButtonType.Mouse ? Input.GetMouseButtonUp((int) MouseButton) : Input.GetKeyUp(KeyboardButton)) {
				if(!Pressed)
					return;
				Pressed = false;
				OnRelease?.Invoke();
			}
		}

#if UNITY_EDITOR
		public void DrawInInspector(SerializedProperty property) {
			InspectorUtils.DrawObject(property, () => {
				EditorGUILayout.PropertyField(property.FindPropertyRelative("ButtonType"), false);
				EditorGUILayout.PropertyField(property.FindPropertyRelative(ButtonType == ButtonType.Mouse ? "MouseButton" : "KeyboardButton"), false);
			});
		}
#endif
	}

	public enum MouseButton {
		Left = 0,
		Right = 1,
		Middle = 2
	}

	public enum ButtonType {
		Keyboard = 0,
		Mouse = 1
	}
}