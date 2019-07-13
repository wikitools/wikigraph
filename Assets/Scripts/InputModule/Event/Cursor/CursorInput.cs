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
		public Action<float> OnXAxisMove;
		public Action<float> OnYAxisMove;
		
		public bool MainAxisOnly;

		protected abstract Vector2 GetCursorPosition();

		public new void CheckForInput() {
			base.CheckForInput();
			if(IsButtonActivated && !ActivationButton.Pressed)
				return;
			var movement = GetCursorPosition();
		
			Action<float>[] axisAction = {OnXAxisMove, OnYAxisMove};
			int mainAxis = Mathf.Abs(movement.x) >= Mathf.Abs(movement.y) ? 0 : 1;
			axisAction[mainAxis].Invoke(movement[mainAxis]);
			if (!MainAxisOnly)
				axisAction[(mainAxis + 1) % 2].Invoke(movement[(mainAxis + 1) % 2]);
			OnMove?.Invoke(movement);
		}
	}
}