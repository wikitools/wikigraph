using System;
using System.Reflection;
using InputModule.Binding;
using InputModule.Processor;
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

		private InputProcessor input;
		private InputBinding binding;
		
		public GraphController Graph { get; private set; }

		void Start() {
			Graph = GetComponent<GraphController>();

			input = Environment == Environment.PC ? (InputProcessor) new PCInputProcessor(Config, PCInputBinding, this) : new CaveInputProcessor(Config, CaveInputBinding, this);
			binding = Environment == Environment.PC ? (InputBinding) PCInputBinding : CaveInputBinding;
			
			// TODO: let user choose the main flystick
			CaveInputBinding.SetPrimaryFlystick(0);
			binding.Init();
		}

		void Update() {
			binding.CheckForInput();
		}
	}

	[Serializable]
	public class InputConfig {
		public float RotationSpeed;
		public Vector2 MaxRotationSpeed;

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