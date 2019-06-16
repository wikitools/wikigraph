using System;
using InputModule.Event;
using InputModule.Event.Button;
using InputModule.Event.Cursor;
using UnityEngine;

namespace InputModule.Binding {
	[Serializable]
	public class PcInputBinding: InputBinding {
		public PCCursor RotationInput;

		public PCButtonAxis MainMovementAxis;
		public PCButtonAxis CrossMovementAxis;
	}
}