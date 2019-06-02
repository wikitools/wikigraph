using System;
using AppInput.Mapping;

namespace AppInput.Event.Button {
	[Serializable]
	public class FlystickButtonEvent: ButtonEvent {
		public FlystickHand Hand;
		public LzwpInput.Flystick.ButtonID Button;

		public FlystickButtonEvent(FlystickHand hand, LzwpInput.Flystick.ButtonID button) {
			Hand = hand;
			Button = button;
			Lzwp.input.flysticks[(int) hand].GetButton(button).OnPress += OnPressed;
		}

		private void Unsubscribe() {
			Lzwp.input.flysticks[(int) Hand].GetButton(Button).OnPress += OnPressed;
		}

		private void Subscribe() {
			Lzwp.input.flysticks[(int) Hand].GetButton(Button).OnPress -= OnPressed;
		}
	}
}