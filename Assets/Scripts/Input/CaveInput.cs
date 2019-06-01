using UnityEngine;

namespace AppInput {
	public class CaveInput : InputEnvironment {
		private InputConfig config;

		public CaveInput(InputConfig config) {
			this.config = config;
		}

		public Vector2 GetRotation() {
			return Vector2.zero;
		}

		public Vector2 GetMovement() {
			return Vector2.zero;
		}
	}
}