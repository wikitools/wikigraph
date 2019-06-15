using AppInput.Binding;
using Controllers;
using UnityEngine;

namespace AppInput.Processor {
	public abstract class InputProcessor {
		protected InputConfig Config;
		protected InputBinding Binding;
		protected InputController Controller;

		public InputProcessor(InputConfig config, InputBinding binding, InputController controller) {
			Config = config;
			Binding = binding;
			Controller = controller;
		}

		public abstract Vector2 GetMovement();
	}
}