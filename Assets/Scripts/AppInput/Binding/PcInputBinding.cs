using System;
using AppInput.Event.Button;
using AppInput.Event.Cursor;
using UnityEngine;

namespace AppInput.Binding {
	[Serializable]
	public class PcInputBinding: InputBinding {
		public PCCursor RotationInput;
		
		public PCButton ForwardMovement;
		public PCButton BackwardMovement;
		public PCButton RightMovement;
		public PCButton LeftMovement;
		public override void OnRotate(Action<Vector2> callback) {
			RotationInput.OnMove += callback;
		}
	}
}