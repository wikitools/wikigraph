using System;
using AppInput.Binding;
using AppInput.Event.Interfaces;
using UnityEngine;

namespace AppInput.Event.Button {
	[Serializable]
	public class FlystickButton: ButtonEvent, InputInitializer {
		public FlystickInstance Instance;
		public LzwpInput.Flystick.ButtonID Button;

		public void Init() {
			Lzwp.input.flysticks[CaveInputBinding.FlystickBinding[Instance]].GetButton(Button).OnPress += () => {
				Pressed = true;
				OnPress?.Invoke();
			};
			Lzwp.input.flysticks[CaveInputBinding.FlystickBinding[Instance]].GetButton(Button).OnRelease += () => {
				Pressed = false;
				OnRelease?.Invoke();
			};
		}
	}
}