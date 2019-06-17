using System;
using InputModule.Event.Axis;
using InputModule.Event.Button;
using InputModule.Event.Interfaces;

namespace InputModule.Event {
	[Serializable]
	public class PCButtonAxis: ButtonPair, InputPoller, InputInitializer {
		public PCButton PositiveButton;
		public PCButton NegativeButton;

		public new void CheckForInput() {
			PositiveButton.CheckForInput();
			NegativeButton.CheckForInput();
			base.CheckForInput();
		}

		public void Init() {
			Init(PositiveButton, NegativeButton);
		}
	}
}