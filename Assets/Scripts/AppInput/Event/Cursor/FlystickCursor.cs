using System;
using AppInput.Event.Button;
using AppInput.Event.Interfaces;
using AppInput.Mapping;
using Inspector;
using UnityEditor;
using UnityEngine;

namespace AppInput.Event.Cursor {
	[Serializable]
	public class FlystickCursor: CursorEvent, CustomInspectorProperty, InputPoller, InputInitializer {
		public FlystickHand Hand;
		public CursorType Cursor;
		public FlystickButton Button;

		public void CheckForInput() {
			base.CheckForInput(Button);
		}

		protected override Vector2 GetCursorPosition() {
			var flystick = Lzwp.input.flysticks[(int) Hand];
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
			base.DrawInInspector(property, new [] {"Hand", "Cursor"});
		}
	}

	public enum CursorType {
		Joystick = 0,
		Pointer = 1
	}
}