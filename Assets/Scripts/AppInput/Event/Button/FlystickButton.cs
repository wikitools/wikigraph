using System;
using AppInput.Event.Interfaces;
using AppInput.Mapping;
using UnityEngine;

namespace AppInput.Event.Button {
	[Serializable]
	public class FlystickButton: ButtonEvent, InputInitializer {
		public FlystickHand Hand;
		public LzwpInput.Flystick.ButtonID Button;

		public void Init() {
			Lzwp.input.flysticks[(int) Hand].GetButton(Button).OnPress += () => {
				Pressed = true;
				OnPress();
			};
			Lzwp.input.flysticks[(int) Hand].GetButton(Button).OnRelease += () => {
				Pressed = false;
				OnRelease();
			};
		}
	}
}