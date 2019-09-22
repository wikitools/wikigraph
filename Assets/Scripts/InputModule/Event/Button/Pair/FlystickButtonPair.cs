using System;
using InputModule.Binding;

namespace InputModule.Event.Button.Pair {
	[Serializable]
	public class FlystickButtonPair: FlystickButton {
		public FlystickButton ModifierButton;

		public override void Init() {
			ModifierButton.Init();
			CaveInputBinding.Flystick(Instance).GetButton(Button).OnPress += () => {
				if(!ModifierButton.Pressed)
					return;
				Pressed = true;
				OnPress?.Invoke();
			};
			CaveInputBinding.Flystick(Instance).GetButton(Button).OnRelease += () => {
				if(!Pressed)
					return;
				Pressed = false;
				OnRelease?.Invoke();
			};
		}
	}
}