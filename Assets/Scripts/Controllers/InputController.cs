using System;
using System.Reflection;
using AppInput;
using AppInput.Event.Button;
using AppInput.Mapping;
using Inspector;
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
		private InputMapping mapping;

		void Start() {
			input = Environment == Environment.PC ? (InputEnvironment) new PCInput(Config, PCMapping) : new CaveInput(Config, CaveMapping);
			mapping = Environment == Environment.PC ? (InputMapping) PCMapping : CaveMapping;
			
			// TODO: need to wait for Lzwp lib initialization
			mapping.Init();
		}

		void Update() {
			mapping.CheckForInput();
			
			Vector2 rotation = input.GetRotation();
			Cave.transform.Rotate(new Vector3(-rotation.y, 0, 0), Space.Self);
			Cave.transform.Rotate(new Vector3(0, rotation.x, 0), Space.World);

			Vector2 movement = input.GetMovement();
			Cave.transform.Translate(movement.y, 0, movement.x, Space.Self);
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
				
				string mappingConfigName = env == Environment.Cave ? "CaveMapping" : "PCMapping";
				var mappingProperty = serializedObject.FindProperty(mappingConfigName);
				InspectorUtils.DrawField(targetType.GetField(mappingConfigName), mappingProperty, serializedObject.targetObject);
				GUI.enabled = true;
			} else if(!typeof(InputMapping).IsAssignableFrom(field.FieldType)) {
				EditorGUILayout.PropertyField(fieldProperty, true);
			}
		}
	}
}