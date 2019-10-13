using System;

namespace InputModule.Event.Button {
	[Serializable]
	public class FlystickButtonPair: FlystickButton {
		private static readonly LzwpInput.Flystick.ButtonID ModifierButton = LzwpInput.Flystick.ButtonID.Fire;
		
		public override void Init() {
			GetButton(ModifierButton).OnRelease += () => {
				if(!Pressed)
					return;
				Pressed = false;
				OnRelease?.Invoke();
			};
			GetButton(Button).OnPress += () => {
				if(!GetButton(ModifierButton).isActive)
					return;
				Pressed = true;
				OnPress?.Invoke();
			};
			GetButton(Button).OnRelease += () => {
				if(!Pressed)
					return;
				if(!GetButton(ModifierButton).isActive)
					return;
				Pressed = false;
				OnRelease?.Invoke();
			};
		}
	}
}