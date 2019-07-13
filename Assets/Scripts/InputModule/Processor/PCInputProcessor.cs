using Controllers;
using InputModule.Binding;
using Services;
using UnityEngine;
using UnityEngine.UI;

namespace InputModule.Processor {
	public class PCInputProcessor: InputProcessor {

		public PCInputProcessor(InputConfig config, PcInputBinding binding, InputController controller) : base(config, binding, controller) {
			binding.RotationInput.OnMove += OnRotate;
			binding.MainMovementAxis.OnMove += dir => OnMove(new Vector2(dir, 0));
			binding.CrossMovementAxis.OnMove += dir => OnMove(new Vector2(0, dir));
			
			binding.NodePointer.OnPointed += OnNodePointed;
			binding.NodeChooser.OnPointed += OnNodeChosen;
			binding.ExitNodeTraverseMode.OnPress += ExitNodeTraverseMode;

			binding.InfographicToggle.OnPress += () => Controller.GraphController.Infographic.SetActive(!Controller.GraphController.Infographic.activeSelf);
		}

		private void OnMove(Vector2 direction) {
			if(Controller.GraphController.GraphMode == GraphMode.NODE_TRAVERSE) return;
			direction *= Config.MovementSpeed;
			EntityTransform.Translate(direction.y, 0, direction.x, Space.Self);
		}

		void OnRotate(Vector2 rawRotation) {
			Vector2 rotation = Config.RotationSpeed * Time.deltaTime * rawRotation;
			Utils.clamp(ref rotation, -Config.MaxRotationSpeed, Config.MaxRotationSpeed);
			EntityTransform.Rotate(new Vector3(-rotation.y, 0, 0), Space.Self);
			EntityTransform.Rotate(new Vector3(0, rotation.x, 0), Space.World);
		}
	}
}
