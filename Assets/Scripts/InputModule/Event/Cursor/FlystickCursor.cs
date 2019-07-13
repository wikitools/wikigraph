using System;
using InputModule.Binding;
using InputModule.Event.Button;
using InputModule.Event.Interfaces;
using Inspector;
using UnityEditor;
using UnityEngine;

namespace InputModule.Event.Cursor {
	[Serializable]
	public class FlystickCursor: CursorInput, FlystickInput {
		public FlystickInstance Instance;
		public FlystickButton Button;

		public override void Init() { }

		protected override Vector2 GetCursorPosition() {
			var flystick = CaveInputBinding.Flystick(Instance);
			return new Vector2(flystick.joysticks[0], flystick.joysticks[1]);
		}
	}
}