using System;
using Interfaces;
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

		public void DrawInInspector(SerializedProperty prop) {
			EditorGUILayout.PropertyField(prop, false);
			if (prop.isExpanded) {
				EditorGUILayout.PropertyField(prop.FindPropertyRelative("buttonType"), false);
				EditorGUILayout.PropertyField(prop.FindPropertyRelative(ButtonType == ButtonType.Mouse ? "mouseButton" : "keyboardButton"), false);
			}
		}
	}

	public enum MouseButton {
		Left = 0,
		Right = 1,
		Middle = 2
	}

	public enum ButtonType {
		Keyboard, Mouse
	}
}