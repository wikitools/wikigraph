using System;

namespace AppInput.Event.Button {
	[Serializable]
	public abstract class ButtonEvent {
		public Action OnPressed;
	}
}