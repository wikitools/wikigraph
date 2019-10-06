using System;
using InputModule.Event.Button;
using InputModule.Event.Interfaces;

namespace InputModule.Event.Axis {
	[Serializable]
	public class FlystickButtonPairAxis: ButtonPair, InputInitializer, FlystickInput {
		public FlystickButtonPair PositiveButton;
		public FlystickButtonPair NegativeButton;

		public void Init() {
			PositiveButton.Init();
			NegativeButton.Init();
			Init(PositiveButton, NegativeButton);
		}
	}
}