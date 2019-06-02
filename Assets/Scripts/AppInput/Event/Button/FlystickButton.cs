using System;
using AppInput.Binding;
using AppInput.Event.Interfaces;
using UnityEngine;

namespace AppInput.Event.Button {
	[Serializable]
	public class FlystickButton: ButtonEvent, InputInitializer {
		public FlystickHand Hand;
		public LzwpInput.Flystick.ButtonID Button;

		public void Init() {
			Lzwp.input.flysticks[(int) Hand].GetButton(Button).OnPress += () => {
				Pressed = true;
				OnPress?.Invoke();
			};
			Lzwp.input.flysticks[(int) Hand].GetButton(Button).OnRelease += () => {
				Pressed = false;
				OnRelease?.Invoke();
			};
		}
	}
}