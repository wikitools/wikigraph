using Controllers;
using UnityEngine;

namespace AppInput {
	public class CaveInput : InputEnvironment {
		public CaveInput(InputConfig config) : base(config) { }

		public override Vector2 GetRotation() {
			return Vector2.zero;
		}

		public override Vector2 GetMovement() {
			return Vector2.zero;
		}
	}
}