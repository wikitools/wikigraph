using AppInput.Binding;
using Controllers;
using UnityEngine;

namespace AppInput.Processor {
	public class PCInputProcessor: InputProcessor {
		private int mainAxisDirection, crossAxisDirection;

		public PCInputProcessor(InputConfig config, PcInputBinding binding, InputController controller) : base(config, binding, controller) {
			binding.OnRotate(OnRotate);
			binding.ForwardMovement.OnPress += () => mainAxisDirection = 1;
			binding.ForwardMovement.OnRelease += () => mainAxisDirection = 0;
			binding.BackwardMovement.OnPress += () => mainAxisDirection = -1;
			binding.BackwardMovement.OnRelease += () => mainAxisDirection = 0;
			binding.LeftMovement.OnPress += () => crossAxisDirection = 1;
			binding.LeftMovement.OnRelease += () => crossAxisDirection = 0;
			binding.RightMovement.OnPress += () => crossAxisDirection = -1;
			binding.RightMovement.OnRelease += () => crossAxisDirection = 0;
		}

		void OnRotate(Vector2 rawRotation) {
			Vector2 rotation = Config.RotationSpeed * Time.deltaTime * rawRotation;
			Utils.clamp(ref rotation, -Config.MaxRotationSpeed, Config.MaxRotationSpeed);
			Controller.Rotate(rotation);
		}

		public override Vector2 GetMovement() {
			return new Vector2(mainAxisDirection * Config.MovementSpeed, crossAxisDirection * Config.MovementSpeed);
		}
	}
}
