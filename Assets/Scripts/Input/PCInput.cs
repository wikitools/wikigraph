using UnityEngine;

namespace AppInput {
	public class PCInput: InputEnvironment {
		private InputConfig config;
		private Vector3 lastMousePos = Vector3.zero;

		public PCInput(InputConfig config) {
			this.config = config;
		}

		public Vector3 GetRotation() {
			Vector3 mousePosDelta = Vector3.zero;
			if (Input.GetMouseButtonDown((int) config.RotationButton)) {
				lastMousePos = Input.mousePosition;
			} else if (Input.GetMouseButton((int) config.RotationButton)) {
				mousePosDelta = (Input.mousePosition - lastMousePos) * config.RotationSpeed * Time.deltaTime;
				Utils.clamp(ref mousePosDelta, -config.MaxRotationSpeed, config.MaxRotationSpeed);
				lastMousePos = Input.mousePosition;
			}
			return mousePosDelta;
		}
	}
}
