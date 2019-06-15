using AppInput.Binding;
using Controllers;
using UnityEngine;

namespace AppInput.Processor {
	public class CaveInputProcessor : InputProcessor {
		public CaveInputProcessor(InputConfig config, CaveInputBinding binding, InputController controller) : base(config, binding, controller) { }

		public override Vector2 GetMovement() {
			return Vector2.zero;
		}
	}
}