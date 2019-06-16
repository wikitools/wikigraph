using System;
using InputModule.Event.Button;
using InputModule.Event.Interfaces;
using Inspector;
using UnityEditor;
using UnityEngine;

namespace InputModule.Event.Cursor {
	[Serializable]
	public class PCCursor: CursorInput {
		public PCButton Button;

		protected override Vector2 GetCursorPosition() {
			return Input.mousePosition;
		}
	}
}