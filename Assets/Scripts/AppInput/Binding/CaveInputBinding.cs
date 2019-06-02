using System;
using AppInput.Event.Button;
using AppInput.Event.Cursor;
using UnityEngine;

namespace AppInput.Binding {
	[Serializable]
	public class CaveInputBinding: InputBinding {
		public FlystickButton Rot;
		public FlystickCursor Crs;
		public override void OnRotate(Action<Vector2> callback) {
		}
	}
}