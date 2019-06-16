using System;
using InputModule.Binding;
using InputModule.Event.Button;
using InputModule.Event.Interfaces;
using UnityEngine;

namespace InputModule.Event.Pointer {
	[Serializable]
	public class FlystickPointer: PointerInput, FlystickInput {
		public FlystickInstance Instance;
		public FlystickButton Button;

		protected override Ray GetPointerRay() {
			var flystick = CaveInputBinding.Flystick(Instance).pose;
			return new Ray(flystick.position, flystick.rotation * Vector3.forward);
		}
	}
}