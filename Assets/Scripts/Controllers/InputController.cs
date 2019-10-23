using System;
using System.Reflection;
using InputModule.Binding;
using InputModule.Processor;
using Inspector;
using UnityEditor;
using UnityEngine;

namespace Controllers {
	public class InputController : MonoBehaviour {
		public Environment Environment;
		
		[EnvironmentField(Environment.PC)]
		public PcInputBinding PCInputBinding;
		[EnvironmentField(Environment.Cave)]
		public CaveInputBinding CaveInputBinding;
		
		[EnvironmentField(Environment.PC)]
		public InputConfig PCConfig;
		[EnvironmentField(Environment.Cave)]
		public InputConfig CaveConfig;

		private InputBinding binding;
		
		[EnvironmentField(Environment.Cave)]
		public GameObject[] Flysticks;
		[EnvironmentField(Environment.Cave)]
		public bool ShowFlystickRays;

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
			if(Environment == Environment.Cave)
				foreach (var flystick in Flysticks)
					flystick.GetComponent<LineRenderer>().enabled = ShowFlystickRays;
			
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
		public float RotationSpeed = 20;
		public Vector2 MaxRotationSpeed = new Vector2(10, 10);

		public float MovementSpeed = .2f;
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
			var environmentAttribute = field.GetCustomAttribute<EnvironmentField>();
			Environment environment = (Environment) targetType.GetField("Environment").GetValue(serializedObject.targetObject);
				
			if (field.Name.EndsWith("InputBinding") && EditorApplication.isPlaying) 
				GUI.enabled = false;
			if(environmentAttribute == null || environmentAttribute.TargetEnvironment == environment)
				DrawField(field.Name, targetType);
			GUI.enabled = true;
		}

		private void DrawField(string name, Type targetType) {
			var mappingProperty = serializedObject.FindProperty(name);
			InspectorUtils.DrawField(targetType.GetField(name), mappingProperty, serializedObject.targetObject);
		}
	}
#endif
}