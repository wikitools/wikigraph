using AppInput.Binding;
using Controllers;
using UnityEngine;

namespace AppInput {
	public class PCInput: InputEnvironment {
		private int mainAxisDirection, crossAxisDirection;

		public PCInput(InputConfig config, PcInputBinding binding, InputController entity) : base(config, binding, entity) {
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
			Entity.Rotate(rotation);
		}

		public override Vector2 GetMovement() {
			return new Vector2(mainAxisDirection * Config.MovementSpeed, crossAxisDirection * Config.MovementSpeed);
		}
	}
}
