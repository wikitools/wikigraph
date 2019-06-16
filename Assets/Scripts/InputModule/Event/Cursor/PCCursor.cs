using System;
using InputModule.Event.Button;
using InputModule.Event.Interfaces;
using Inspector;
using UnityEditor;
using UnityEngine;

namespace InputModule.Event.Cursor {
	[Serializable]
	public class PCCursor: CursorInput, CustomInspectorProperty, InputPoller, InputInitializer {
		public PCButton Button;

		public void DrawInInspector(SerializedProperty property) {
			base.DrawInInspector(property);
		}

		public void CheckForInput() {
			base.CheckForInput(Button);
		}

		public void Init() {
			base.Init(Button);
		}

		protected override Vector2 GetCursorPosition() {
			return Input.mousePosition;
		}
	}
}