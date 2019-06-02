using System;
using Inspector;
using UnityEditor;
using UnityEngine;

namespace AppInput.Event.Button {
	[Serializable]
	public class PCButtonEvent: ButtonEvent, CustomInspectorProperty {
		public ButtonType ButtonType;
		public MouseButton MouseButton;
		public KeyCode KeyboardButton;

		public PCButtonEvent(MouseButton mouseButton) {
			ButtonType = ButtonType.Mouse;
			MouseButton = mouseButton;
		}

		public PCButtonEvent(KeyCode keyboardButton) {
			ButtonType = ButtonType.Keyboard;
			KeyboardButton = keyboardButton;
		}

		public void Update() {
			if (OnPressed != null && (ButtonType == ButtonType.Mouse ? Input.GetMouseButtonDown((int) MouseButton) : Input.GetKeyDown(KeyboardButton))) {
				OnPressed();
			}
		}

		public void DrawInInspector(SerializedProperty property) {
			EditorGUILayout.PropertyField(property, false);
			if (property.isExpanded) {
				EditorGUILayout.PropertyField(property.FindPropertyRelative("ButtonType"), false);
				EditorGUILayout.PropertyField(property.FindPropertyRelative(ButtonType == ButtonType.Mouse ? "MouseButton" : "KeyboardButton"), false);
			}
		}
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