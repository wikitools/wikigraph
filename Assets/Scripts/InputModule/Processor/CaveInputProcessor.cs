using Controllers;
using InputModule.Binding;
using UnityEngine;

namespace InputModule.Processor {
	public class CaveInputProcessor : InputProcessor {
		public CaveInputProcessor(InputConfig config, CaveInputBinding binding, InputController controller) : base(config, binding, controller) { }
	}
}