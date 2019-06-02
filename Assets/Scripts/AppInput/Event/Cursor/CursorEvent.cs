using System;
using System.Collections.Generic;
using AppInput.Event.Button;
using Inspector;
using UnityEditor;
using UnityEngine;
using System.Linq;
using AppInput.Event.Interfaces;

namespace AppInput.Event.Cursor {
	public abstract class CursorEvent {
		public bool IsButtonActivated;
		public Action<Vector2> OnMove;
		
		protected Vector2 LastMousePos = Vector2.zero;

		protected abstract Vector2 GetCursorPosition();

		public void Init(ButtonEvent button) {
			if (IsButtonActivated)
				button.OnPress += () => LastMousePos = GetCursorPosition();
		}

		public void CheckForInput(ButtonEvent button) {
			if (IsButtonActivated) {
				if(typeof(InputPoller).IsAssignableFrom(button.GetType()))
					(button as InputPoller).CheckForInput();
				if(!button.Pressed)
					return;
			}
			OnMove(GetCursorPosition() - LastMousePos);
			LastMousePos = Input.mousePosition;
		}
		
		protected void DrawInInspector(SerializedProperty property, string[] additionalFields = null) {
			EditorGUILayout.PropertyField(property, false);
			if (property.isExpanded) {
				EditorGUI.indentLevel++;
				additionalFields?.ToList().ForEach(field => EditorGUILayout.PropertyField(property.FindPropertyRelative(field), false));
				EditorGUILayout.PropertyField(property.FindPropertyRelative("IsButtonActivated"), false);
				if (IsButtonActivated) {
					var buttonProperty = property.FindPropertyRelative("Button");
					InspectorUtils.DrawField(GetType().GetField("Button"), buttonProperty, this);
				}
				EditorGUI.indentLevel--;
			}
		}
	}
}