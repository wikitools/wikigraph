using System;
using Controllers;
using InputModule.Binding;

namespace InputModule.Processor {
	public class CaveInputProcessor : InputProcessor {
		public CaveInputProcessor(InputConfig config, CaveInputBinding binding, InputController controller) : base(config, binding, controller) {
			binding.MovementJoystick.OnXAxisMove += amount => EntityTransform.Rotate(0, amount * Config.RotationSpeed, 0);
			binding.MovementJoystick.OnYAxisMove += OnMovementJoystickYAxisMove;
			binding.NodePointer.OnPointed += OnNodePointed;
			binding.NodeChooser.OnPointed += OnNodeChosen;
			binding.ExitNodeTraverseMode.OnPress += ExitNodeTraverseMode;
			binding.RedoButton.OnPress += RedoUserAction;
			binding.UndoButton.OnPress += UndoUserAction;
		}

		private void OnMovementJoystickYAxisMove(float amount) {
			if (Controller.GraphController.GraphMode.Value == GraphMode.FREE_FLIGHT)
				EntityTransform.Translate(0, 0, amount * Config.MovementSpeed);
			else
				OnConnectionScrolled(Math.Sign(amount));
		}
	}
}