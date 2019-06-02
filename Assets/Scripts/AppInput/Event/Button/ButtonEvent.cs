using System;

namespace AppInput.Event.Button {
	public abstract class ButtonEvent {
		public Action OnPress;
		public Action OnRelease;

		public bool Pressed { get; protected set; }
	}
}