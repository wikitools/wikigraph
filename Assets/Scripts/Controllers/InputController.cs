using System;
using System.Reflection;
using AppInput;
using AppInput.Event.Button;
using AppInput.Mapping;
using UnityEditor;
using UnityEngine;

namespace Controllers {
	public class InputController : MonoBehaviour {
		public GameObject Cave;
		public InputConfig Config;

		public Environment Environment;
		public PCInputMapping PCMapping;
		public CaveInputMapping CaveMapping;

		private InputEnvironment input;

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

	[CustomEditor(typeof(InputController))]
	[CanEditMultipleObjects]
	public class InputConfigEditor : Editor {
		public override void OnInspectorGUI() {
			serializedObject.Update();
			SerializedProperty prop = serializedObject.GetIterator();
			Type type = serializedObject.targetObject.GetType();
			
			if (prop.NextVisible(true)) {
				do {
					FieldInfo field = type.GetField(prop.name);
					if (field == null) {
						continue;
					}
					HandleField(prop, field);
				} while (prop.NextVisible(false));
			}
			serializedObject.ApplyModifiedProperties();
		}

		private void HandleField(SerializedProperty prop, FieldInfo field) {
			if (field.FieldType == typeof(PCButtonEvent)) {
				(field.GetValue(serializedObject.targetObject) as PCButtonEvent).DrawInInspector(prop);
			} else if (field.FieldType == typeof(Environment)) {
				EditorGUILayout.PropertyField(prop, true);
				Environment env = (Environment) field.GetValue(serializedObject.targetObject);
				EditorGUILayout.PropertyField(serializedObject.FindProperty(env == Environment.Cave ? "CaveMapping" : "PCMapping"), true);
			} else if(!typeof(InputMapping).IsAssignableFrom(field.FieldType)) {
				EditorGUILayout.PropertyField(prop, true);
			}
		}
	}

	public enum Environment {
		PC,
		Cave
	}

	#endregion
}