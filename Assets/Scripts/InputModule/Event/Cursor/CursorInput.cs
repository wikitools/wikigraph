using System;
using System.Collections.Generic;
using Inspector;
using UnityEditor;
using UnityEngine;
using System.Linq;
using InputModule.Event.Button;
using InputModule.Event.Interfaces;

namespace InputModule.Event.Cursor {
	public abstract class CursorInput: ActivatedInput, InputPoller {
		public Action<Vector2> OnMove;
		
		protected Vector2 LastMousePos = Vector2.zero;

		protected abstract Vector2 GetCursorPosition();

		public override void Init() {
			if (IsButtonActivated)
				ActivationButton.OnPress += () => LastMousePos = GetCursorPosition();
		}

		public new void CheckForInput() {
			base.CheckForInput();
			if(IsButtonActivated && !ActivationButton.Pressed)
				return;
			OnMove?.Invoke(GetCursorPosition() - LastMousePos);
			LastMousePos = Input.mousePosition;
		}
	}
}