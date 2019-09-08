using System;
using InputModule.Binding;
using InputModule.Event.Interfaces;
using UnityEngine;

namespace InputModule.Event.Button {
	[Serializable]
	public class FlystickButton : ButtonInput, InputInitializer, FlystickInput {
		public FlystickInstance Instance;
		public LzwpInput.Flystick.ButtonID Button;

		public void Init() {
			CaveInputBinding.Flystick(Instance).GetButton(Button).OnPress += () => {
				Pressed = true;
				OnPress?.Invoke();
			};
			CaveInputBinding.Flystick(Instance).GetButton(Button).OnRelease += () => {
				Pressed = false;
				OnRelease?.Invoke();
			};
		}
	}
}