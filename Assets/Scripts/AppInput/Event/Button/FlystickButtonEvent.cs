using System;
using AppInput.Mapping;

namespace AppInput.Event.Button {
	[Serializable]
	public class FlystickButtonEvent: ButtonEvent {
		public FlystickHand Hand;
		public LzwpInput.Flystick.ButtonID Button;

		public FlystickButtonEvent(FlystickHand hand, LzwpInput.Flystick.ButtonID button) {
			this.Hand = hand;
			this.Button = button;
			Lzwp.input.flysticks[(int) hand].GetButton(button).OnPress += OnPressed;
		}
	}
}