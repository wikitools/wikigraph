using AppInput;
using UnityEngine;

public class InputController : MonoBehaviour {
	public GameObject Cave;
	public Environment Environment;
	public InputConfig Config;

	private InputEnvironment input;

	// Use this for initialization
	void Start() {
		input = Environment == Environment.PC ? (InputEnvironment) new PCInput(Config) : new CaveInput(Config);
	}

	// Update is called once per frame
	void Update() {
		Vector2 rotation = input.GetRotation();
		Cave.transform.Rotate(new Vector3(-rotation.y, 0, 0), Space.Self);
		Cave.transform.Rotate(new Vector3(0, rotation.x, 0), Space.World);

		Vector2 movement = input.GetMovement();
		Cave.transform.Translate(movement.y, 0, movement.x, Space.Self);
	}
}

#region InputModels

[System.Serializable]
public class InputConfig {
	public float RotationSpeed;
	public Vector2 MaxRotationSpeed;
	public MouseButton RotationButton;

	public float MovementSpeed;
}

public enum Environment {
	PC,
	CAVE
}

public enum MouseButton {
	LEFT = 0,
	RIGHT = 1,
	MIDDLE = 2
}

#endregion