using System;
using InputModule.Event.Button;
using UnityEngine;

namespace InputModule.Event.Cursor {
	[Serializable]
	public class PCCursor : CursorInput {
		public PCButton Button;

		protected Vector2 LastMousePos = Vector2.zero;

		public override void Init() {
			if (IsButtonActivated)
				ActivationButton.OnPress += () => LastMousePos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
		}

		protected override Vector2 GetCursorPosition() {
			var movement = new Vector2(Input.mousePosition.x, Input.mousePosition.y) - LastMousePos;
			LastMousePos = Input.mousePosition;
			return movement;
		}
	}
}