using UnityEngine;

namespace AppInput {
	public class CaveInput : InputEnvironment {
		private InputConfig config;

		public CaveInput(InputConfig config) {
			this.config = config;
		}

		public Vector3 GetRotation() {
			return Vector3.zero;
		}
	}
}