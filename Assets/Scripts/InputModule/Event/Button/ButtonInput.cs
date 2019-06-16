using System;

namespace InputModule.Event.Button {
	public abstract class ButtonInput {
		public Action OnPress;
		public Action OnRelease;

		public bool Pressed { get; protected set; }
	}
}