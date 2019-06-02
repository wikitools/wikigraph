using System;
using System.Reflection;
using AppInput;
using AppInput.Binding;
using AppInput.Event.Button;
using Inspector;
using UnityEditor;
using UnityEngine;

namespace Controllers {
	public class InputController : MonoBehaviour {
		public GameObject Entity;
		public InputConfig Config;

		public Environment Environment;
		public PcInputBinding PCInputBinding;
		public CaveInputBinding CaveInputBinding;

		private InputEnvironment input;
		private InputBinding binding;

		void Start() {
			input = Environment == Environment.PC ? (InputEnvironment) new PCInput(Config, PCInputBinding, this) : new CaveInput(Config, CaveInputBinding, this);
			binding = Environment == Environment.PC ? (InputBinding) PCInputBinding : CaveInputBinding;
			
			// TODO: need to wait for Lzwp lib initialization
			binding.Init();
		}

		public void Rotate(Vector2 rawRotation) {
			Vector2 rotation = Config.RotationSpeed * Time.deltaTime * rawRotation;
			Utils.clamp(ref rotation, -Config.MaxRotationSpeed, Config.MaxRotationSpeed);
			Entity.transform.Rotate(new Vector3(-rotation.y, 0, 0), Space.Self);
			Entity.transform.Rotate(new Vector3(0, rotation.x, 0), Space.World);
		}

		void Update() {
			binding.CheckForInput();

			Vector2 movement = input.GetMovement();
			Entity.transform.Translate(movement.y, 0, movement.x, Space.Self);
		}
	}

	[Serializable]
	public class InputConfig {
		public float RotationSpeed;
		public Vector2 MaxRotationSpeed;
		public MouseButton RotationButton;

		public float MovementSpeed;
	}

	public enum Environment {
		PC = 0,
		Cave = 1
	}

	[CustomEditor(typeof(InputController))]
	[CanEditMultipleObjects]
	public class InputConfigEditor : Editor {
		public override void OnInspectorGUI() {
			serializedObject.Update();
			SerializedProperty targetFieldProperty = serializedObject.GetIterator();
			Type targetType = serializedObject.targetObject.GetType();
			if (targetFieldProperty.NextVisible(true)) {
				do {
					HandleField(targetFieldProperty, targetType);
				} while (targetFieldProperty.NextVisible(false));
			}
			serializedObject.ApplyModifiedProperties();
		}

		private void HandleField(SerializedProperty fieldProperty, Type targetType) {
			FieldInfo field = targetType.GetField(fieldProperty.name);
			if (field == null) {
				return;
			}
			if (field.FieldType == typeof(Environment)) {
				if(EditorApplication.isPlaying)
					GUI.enabled = false;
				EditorGUILayout.PropertyField(fieldProperty, true);
				Environment env = (Environment) field.GetValue(serializedObject.targetObject);
				
				string mappingConfigName = env == Environment.Cave ? "CaveInputBinding" : "PCInputBinding";
				var mappingProperty = serializedObject.FindProperty(mappingConfigName);
				InspectorUtils.DrawField(targetType.GetField(mappingConfigName), mappingProperty, serializedObject.targetObject);
				GUI.enabled = true;
			} else if(!typeof(InputBinding).IsAssignableFrom(field.FieldType)) {
				EditorGUILayout.PropertyField(fieldProperty, true);
			}
		}
	}
}