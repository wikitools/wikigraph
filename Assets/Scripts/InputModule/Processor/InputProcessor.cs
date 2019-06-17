using Controllers;
using InputModule.Binding;
using UnityEngine;

namespace InputModule.Processor {
	public abstract class InputProcessor {
		protected InputConfig Config;
		protected InputBinding Binding;
		protected InputController Controller;

		public InputProcessor(InputConfig config, InputBinding binding, InputController controller) {
			Config = config;
			Binding = binding;
			Controller = controller;
		}
	}
}