using System;
using InputModule.Binding;
using InputModule.Event.Button;
using InputModule.Event.Interfaces;
using Inspector;
using UnityEditor;
using UnityEngine;

namespace InputModule.Event.Cursor {
	[Serializable]
	public class FlystickCursor: CursorInput, CustomInspectorProperty, InputPoller, InputInitializer {
		public FlystickInstance Instance;
		public CursorType Cursor;
		public FlystickButton Button;

		public void CheckForInput() {
			base.CheckForInput(Button);
		}

		protected override Vector2 GetCursorPosition() {
			var flystick = CaveInputBinding.Flystick(Instance);
			if(Cursor == CursorType.Joystick) {
				return new Vector2(flystick.joysticks[0], flystick.joysticks[1]);
			}
			// TODO add another layer for raycasting
			return new Vector2(); // flystick.pose.rotation * Vector3.forward
		}

		public void Init() {
			base.Init(Button);
		}

		public void DrawInInspector(SerializedProperty property) {
			base.DrawInInspector(property, new [] {"Instance", "Cursor"});
		}
	}

	public enum CursorType {
		Joystick = 0,
		Pointer = 1
	}
}