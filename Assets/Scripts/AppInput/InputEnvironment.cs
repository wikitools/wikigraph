using AppInput.Mapping;
using Controllers;
using UnityEngine;

namespace AppInput {
	public abstract class InputEnvironment {
		protected InputConfig Config;
		protected InputMapping Mapping;

		public InputEnvironment(InputConfig config, InputMapping mapping) {
			Config = config;
			Mapping = mapping;
		}

		public abstract Vector2 GetRotation();

		public abstract Vector2 GetMovement();
	}
}