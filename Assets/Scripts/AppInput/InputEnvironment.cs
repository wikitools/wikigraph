using Controllers;
using UnityEngine;

namespace AppInput {
	public abstract class InputEnvironment {
		protected InputConfig config;

		public InputEnvironment(InputConfig config) {
			this.config = config;
		}

		public abstract Vector2 GetRotation();

		public abstract Vector2 GetMovement();
	}
}