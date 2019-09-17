using System;
using System.Reflection;
using InputModule.Binding;
using InputModule.Processor;
using Inspector;
using UnityEditor;
using UnityEngine;

namespace Controllers {
	public class InputController : MonoBehaviour {
		public InputConfig PCConfig;
		public InputConfig CaveConfig;

		public Environment Environment;
		public PcInputBinding PCInputBinding;
		public CaveInputBinding CaveInputBinding;

		private InputBinding binding;

		public NetworkController NetworkController { get; private set; }
		public CameraController CameraController { get; private set; }
		public GraphController GraphController { get; private set; }
		public NodeController NodeController { get; private set; }
		public ConnectionController ConnectionController { get; private set; }
		public HistoryController HistoryController { get; private set; }

		void Awake() {
			NetworkController = GetComponent<NetworkController>();
			CameraController = GetComponent<CameraController>();
			GraphController = GetComponent<GraphController>();
			NodeController = GetComponent<NodeController>();
			ConnectionController = GetComponent<ConnectionController>();
			HistoryController = GetComponent<HistoryController>();
		}

		void Start() {
			if (!NetworkController.IsServer())
				return;
			InputProcessor input = Environment == Environment.PC
				? (InputProcessor) new PCInputProcessor(PCConfig, PCInputBinding, this)
				: new CaveInputProcessor(CaveConfig, CaveInputBinding, this);
			binding = Environment == Environment.PC ? (InputBinding) PCInputBinding : CaveInputBinding;

			// TODO: let user choose the main flystick
			CaveInputBinding.SetPrimaryFlystick(0);
			binding.Init();
		}

		void Update() {
			if (Environment == Environment.PC && Input.GetKeyDown(KeyCode.Delete)) {
				NetworkController.CloseClient();
				Application.Quit();
			}

			if (!NetworkController.IsServer())
				return;
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

#if UNITY_EDITOR
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
				if (EditorApplication.isPlaying)
					GUI.enabled = false;
				EditorGUILayout.PropertyField(fieldProperty, true);
				Environment env = (Environment) field.GetValue(serializedObject.targetObject);

				DrawField(env == Environment.Cave ? "CaveInputBinding" : "PCInputBinding", targetType);
				DrawField(env == Environment.Cave ? "CaveConfig" : "PCConfig", targetType);
				GUI.enabled = true;
			}
		}

		private void DrawField(string name, Type targetType) {
			var mappingProperty = serializedObject.FindProperty(name);
			InspectorUtils.DrawField(targetType.GetField(name), mappingProperty, serializedObject.targetObject);
		}
	}
#endif
}