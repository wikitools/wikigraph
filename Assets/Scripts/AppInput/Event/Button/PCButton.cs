using System;
using AppInput.Event.Interfaces;
using Inspector;
using UnityEditor;
using UnityEngine;

namespace AppInput.Event.Button {
	[Serializable]
	public class PCButton: ButtonEvent, CustomInspectorProperty, InputPoller {
		public ButtonType ButtonType;
		public MouseButton MouseButton;
		public KeyCode KeyboardButton;

		public void CheckForInput() {
			if (ButtonType == ButtonType.Mouse ? Input.GetMouseButtonDown((int) MouseButton) : Input.GetKeyDown(KeyboardButton)) {
				Pressed = true;
				OnPress?.Invoke();
			}
			if (ButtonType == ButtonType.Mouse ? Input.GetMouseButtonUp((int) MouseButton) : Input.GetKeyUp(KeyboardButton)) {
				Pressed = false;
				OnRelease?.Invoke();
			}
		}

		public void DrawInInspector(SerializedProperty property) {
			EditorGUILayout.PropertyField(property, false);
			if (property.isExpanded) {
				EditorGUI.indentLevel++;
				EditorGUILayout.PropertyField(property.FindPropertyRelative("ButtonType"), false);
				EditorGUILayout.PropertyField(property.FindPropertyRelative(ButtonType == ButtonType.Mouse ? "MouseButton" : "KeyboardButton"), false);
				EditorGUI.indentLevel--;
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