using System;
using Controllers;
using InputModule.Binding;
using Inspector;
using UnityEngine;

namespace InputModule.Processor {
	public class CaveInputProcessor : InputProcessor {
		private readonly CaveInputBinding binding;

		public CaveInputProcessor(InputConfig config, CaveInputBinding binding, InputController controller) : base(config, controller) {
			this.binding = binding;

			binding.MovementJoystick.OnXAxisMove += Rotate;
			binding.MovementJoystick.OnYAxisMove += OnMovementJoystickYAxisMove;
			binding.NodePointer.OnPointed += OnNodePointed;
			binding.NodeChooser.OnPointed += OnNodeChosen;
			binding.ExitNodeTraverseMode.OnPress += ExitNodeTraverseMode;
			binding.HistoryAxis.OnInputChange += BindHistoryEvents;
			binding.InfoSpaceToggle.OnPress += ToggleInfoSpace;

			binding.ConnectionsScrollAxis.OnInputChange += OnScrollInputChanged;
			binding.OperatorConsoleToggle.OnRelease += ToggleOperatorConsole;
		}

		private void Rotate(float amount) {
			EntityTransform.Rotate(0, Mathf.Clamp(amount * Config.RotationSpeed, -Config.MaxRotationSpeed.x, Config.MaxRotationSpeed.x), 0, Space.World);
		}

		private void OnMovementJoystickYAxisMove(float amount) {
			if (Controller.GraphController.GraphMode.Value != GraphMode.FREE_FLIGHT || Controller.BlockType.HasValue)
				return;
			var translation = CaveInputBinding.Flystick(binding.MovementJoystick.Instance).pose.rotation * Vector3.forward;
			EntityTransform.Translate(Config.MovementSpeed * amount * translation, Space.World);
		}
	}
}