using Controllers;
using UnityEngine;

namespace AppInput {
	public class PCInput: InputEnvironment {
		private Vector3 lastMousePos = Vector3.zero;

		private int mainAxisDirection, crossAxisDirection;

		public PCInput(InputConfig config) : base(config) { }

		public override Vector2 GetRotation() {
			Vector2 mousePosDelta = Vector3.zero;
			if (Input.GetMouseButtonDown((int) config.RotationButton)) {
				lastMousePos = Input.mousePosition;
			} else if (Input.GetMouseButton((int) config.RotationButton)) {
				mousePosDelta = (Input.mousePosition - lastMousePos) * config.RotationSpeed * Time.deltaTime;
				Utils.clamp(ref mousePosDelta, -config.MaxRotationSpeed, config.MaxRotationSpeed);
				lastMousePos = Input.mousePosition;
			}
			return mousePosDelta;
		}

		public override Vector2 GetMovement() {
			CheckAxisDirection(ref mainAxisDirection, KeyCode.UpArrow, KeyCode.DownArrow);
			CheckAxisDirection(ref crossAxisDirection, KeyCode.RightArrow, KeyCode.LeftArrow);
			return new Vector2(mainAxisDirection * config.MovementSpeed, crossAxisDirection * config.MovementSpeed);
		}

		private void CheckAxisDirection(ref int state, KeyCode forward, KeyCode backward) {
			if(Input.GetKeyDown(forward) || Input.GetKeyDown(backward))
				state = Input.GetKeyDown(forward) ? 1 : -1;
			if(Input.GetKeyUp(forward) || Input.GetKeyUp(backward))
				state = 0;
		}
	}
}
