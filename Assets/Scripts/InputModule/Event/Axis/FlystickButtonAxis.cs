using System;
using InputModule.Event.Button;
using InputModule.Event.Interfaces;

namespace InputModule.Event.Axis {
	[Serializable]
	public class FlystickButtonAxis : ButtonPair, InputInitializer, FlystickInput {
		public FlystickButton PositiveButton;
		public FlystickButton NegativeButton;

		public void Init() {
			PositiveButton.Init();
			NegativeButton.Init();
			Init(PositiveButton, NegativeButton);
		}
	}
}