using System;
using Controllers;
using InputModule.Binding;
using UnityEngine;

namespace InputModule.Processor {
	public class CaveInputProcessor : InputProcessor {
		private readonly CaveInputBinding binding;

		public CaveInputProcessor(InputConfig config, CaveInputBinding binding, InputController controller) : base(config, binding, controller) {
			this.binding = binding;
			
			binding.MovementJoystick.OnXAxisMove += amount => EntityTransform.Rotate(0, amount * Config.RotationSpeed, 0, Space.World);
			binding.MovementJoystick.OnYAxisMove += OnMovementJoystickYAxisMove;
			binding.NodePointer.OnPointed += OnNodePointed;
			binding.NodeChooser.OnPointed += OnNodeChosen;
			binding.ExitNodeTraverseMode.OnPress += ExitNodeTraverseMode;
			binding.RedoButton.OnPress += RedoUserAction;
			binding.UndoButton.OnPress += UndoUserAction;
		}

		private void OnMovementJoystickYAxisMove(float amount) {
			if (Controller.GraphController.GraphMode.Value == GraphMode.FREE_FLIGHT) {
				var translation = CaveInputBinding.Flystick(binding.MovementJoystick.Instance).pose.rotation * Vector3.forward;
				EntityTransform.Translate(Config.MovementSpeed * amount * translation, Space.World);
			} else {
				Controller.ConnectionController.OnAdvanceScrollInput(Math.Abs(amount) >= 1 ? 1 : 0 * Math.Sign(amount));
			}
		}
	}
}