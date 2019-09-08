using System;
using InputModule.Event.Button;
using UnityEngine;

namespace InputModule.Event.Pointer {
	[Serializable]
	public class PCPointer : PointerInput {
		public PCButton Button;

		protected override Ray GetPointerRay() {
			return Camera.main.ScreenPointToRay(Input.mousePosition);
		}
	}
}