using System;
using System.Collections.Generic;
using Inspector;
using UnityEditor;
using UnityEngine;
using System.Linq;
using InputModule.Event.Button;
using InputModule.Event.Interfaces;

namespace InputModule.Event.Cursor {
	public abstract class CursorInput {
		public bool IsButtonActivated;
		public Action<Vector2> OnMove;
		
		protected Vector2 LastMousePos = Vector2.zero;

		protected abstract Vector2 GetCursorPosition();

		public void Init(ButtonInput button) {
			if (IsButtonActivated)
				button.OnPress += () => LastMousePos = GetCursorPosition();
		}

		public void CheckForInput(ButtonInput button) {
			if (IsButtonActivated) {
				if(typeof(InputPoller).IsAssignableFrom(button.GetType()))
					(button as InputPoller).CheckForInput();
				if(!button.Pressed)
					return;
			}
			OnMove?.Invoke(GetCursorPosition() - LastMousePos);
			LastMousePos = Input.mousePosition;
		}
		
		protected void DrawInInspector(SerializedProperty property, string[] additionalFields = null) {
			InspectorUtils.DrawObject(property, () => {
				additionalFields?.ToList().ForEach(field => EditorGUILayout.PropertyField(property.FindPropertyRelative(field), false));
				EditorGUILayout.PropertyField(property.FindPropertyRelative("IsButtonActivated"), false);
				if (IsButtonActivated) {
					var buttonProperty = property.FindPropertyRelative("Button");
					InspectorUtils.DrawField(GetType().GetField("Button"), buttonProperty, this);
				}
			});
		}
	}
}