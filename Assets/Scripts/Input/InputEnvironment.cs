using UnityEngine;

namespace AppInput {
	public interface InputEnvironment {
		Vector2 GetRotation();
		Vector2 GetMovement();

	}
}