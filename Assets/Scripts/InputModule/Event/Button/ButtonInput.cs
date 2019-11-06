using System;
using InputModule.Event.Interfaces;

namespace InputModule.Event.Button {
	public abstract class ButtonInput: InputBlocker {
		public Action OnPress;
		public Action OnRelease;
		protected bool Blocked;

		public bool Pressed { get; protected set; }
		
		public void SetBlocked(bool blocked) {
			Blocked = blocked;
		}
	}
}