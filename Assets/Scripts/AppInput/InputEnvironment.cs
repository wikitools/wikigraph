using AppInput.Binding;
using Controllers;
using UnityEngine;

namespace AppInput {
	public abstract class InputEnvironment {
		protected InputConfig Config;
		protected InputBinding Binding;
		protected InputController Entity;

		public InputEnvironment(InputConfig config, InputBinding binding, InputController entity) {
			Config = config;
			Binding = binding;
			Entity = entity;
		}

		public abstract Vector2 GetMovement();
	}
}