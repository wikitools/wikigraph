using AppInput.Binding;
using Controllers;
using UnityEngine;

namespace AppInput {
	public class CaveInput : InputEnvironment {
		public CaveInput(InputConfig config, CaveInputBinding binding, InputController entity) : base(config, binding, entity) { }

		public override Vector2 GetMovement() {
			return Vector2.zero;
		}
	}
}