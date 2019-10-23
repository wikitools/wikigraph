using System;
using System.Collections.Generic;
using System.Linq;
using InputModule.Binding;
using InputModule.Event.Interfaces;
using UnityEngine;

namespace InputModule.Event.Button {
	[Serializable]
	public class FlystickButton : ButtonInput, InputInitializer, FlystickInput {
		public FlystickInstance Instance;
		public LzwpInput.Flystick.ButtonID Button;

		private static readonly List <LzwpInput.Flystick.ButtonID> BUTTONS = new List<LzwpInput.Flystick.ButtonID>(new[] {
			LzwpInput.Flystick.ButtonID.Button1,
			LzwpInput.Flystick.ButtonID.Button2,
			LzwpInput.Flystick.ButtonID.Button3,
			LzwpInput.Flystick.ButtonID.Button4,
			LzwpInput.Flystick.ButtonID.Fire
		});
		public virtual void Init() {
			GetButton(Button).OnPress += () => {
				if(BUTTONS.Where(button => button != Button).Any(id => GetButton(id).isActive))
					return;
				Pressed = true;
				OnPress?.Invoke();
			};
			GetButton(Button).OnRelease += () => {
				if(!Pressed)
					return;
				if(BUTTONS.Where(button => button != Button).Any(id => GetButton(id).isActive))
					return;
				Pressed = false;
				OnRelease?.Invoke();
			};
		}

		protected LzwpInput.Button GetButton(LzwpInput.Flystick.ButtonID id) => CaveInputBinding.Flystick(Instance).GetButton(id);
	}
}