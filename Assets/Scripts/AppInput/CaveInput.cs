using AppInput.Mapping;
using Controllers;
using UnityEngine;

namespace AppInput {
	public class CaveInput : InputEnvironment {
		public CaveInput(InputConfig config, CaveInputMapping mapping) : base(config, mapping) { }

		public override Vector2 GetRotation() {
			return Vector2.zero;
		}

		public override Vector2 GetMovement() {
			return Vector2.zero;
		}
	}
}