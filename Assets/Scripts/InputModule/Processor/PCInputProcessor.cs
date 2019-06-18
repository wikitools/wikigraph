using Controllers;
using InputModule.Binding;
using Services;
using UnityEngine;
using UnityEngine.UI;

namespace InputModule.Processor {
	public class PCInputProcessor: InputProcessor {
		private readonly Logger<PCInputProcessor> LOGGER = new Logger<PCInputProcessor>();

		public PCInputProcessor(InputConfig config, PcInputBinding binding, InputController controller) : base(config, binding, controller) {
			binding.RotationInput.OnMove += OnRotate;
			binding.NodePointer.OnPointed += OnNodePointed;
			binding.NodeChooser.OnPointed += OnNodeChosen;
			binding.MainMovementAxis.OnMove += dir => OnMove(new Vector2(dir, 0));
			binding.CrossMovementAxis.OnMove += dir => OnMove(new Vector2(0, dir));
		}

		private void OnMove(Vector2 direction) {
			direction *= Config.MovementSpeed;
			Controller.CameraController.Entity.transform.Translate(direction.y, 0, direction.x, Space.Self);
		}

		void OnRotate(Vector2 rawRotation) {
			Vector2 rotation = Config.RotationSpeed * Time.deltaTime * rawRotation;
			Utils.clamp(ref rotation, -Config.MaxRotationSpeed, Config.MaxRotationSpeed);
			Controller.CameraController.Entity.transform.Rotate(new Vector3(-rotation.y, 0, 0), Space.Self);
			Controller.CameraController.Entity.transform.Rotate(new Vector3(0, rotation.x, 0), Space.World);
		}
	}
}
