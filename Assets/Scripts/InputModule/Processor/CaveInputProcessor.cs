using Controllers;
using InputModule.Binding;
using UnityEngine;

namespace InputModule.Processor {
	public class CaveInputProcessor : InputProcessor {
		public CaveInputProcessor(InputConfig config, CaveInputBinding binding, InputController controller) : base(config, binding, controller) {
			binding.MovementJoystick.OnXAxisMove += amount => EntityTransform.Rotate(0, amount * Config.RotationSpeed, 0);
			binding.MovementJoystick.OnYAxisMove += amount => EntityTransform.Translate(0, 0, amount * Config.MovementSpeed);
			binding.NodePointer.OnPointed += OnNodePointed;
			binding.NodeChooser.OnPointed += OnNodeChosen;
			binding.ExitNodeTraverseMode.OnPress += ExitNodeTraverseMode;
		}
	}
}