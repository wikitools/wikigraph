using System;
using System.Reflection;
using InputModule.Attributes;
using InputModule.Binding;
using InputModule.Event.Interfaces;
using InputModule.Processor;
using Inspector;
using UnityEditor;
using UnityEngine;
using System.Linq;

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

		public Transform Eyes;
		public InputBlockType? BlockType { get; private set; } = InputBlockType.INFO_SPACE;

		public NetworkController NetworkController { get; private set; }
		public CameraController CameraController { get; private set; }
		public GraphController GraphController { get; private set; }
		public NodeController NodeController { get; private set; }
		public ConnectionController ConnectionController { get; private set; }
		public HistoryController HistoryController { get; private set; }
		public ConsoleWindowController ConsoleController { get; private set; }
		public InfoSpaceController InfoSpaceController { get; private set; }

		void Awake() {
			NetworkController = GetComponent<NetworkController>();
			CameraController = GetComponent<CameraController>();
			GraphController = GetComponent<GraphController>();
			NodeController = GetComponent<NodeController>();
			ConnectionController = GetComponent<ConnectionController>();
			HistoryController = GetComponent<HistoryController>();
			ConsoleController = (ConsoleWindowController) Resources.FindObjectsOfTypeAll(typeof(ConsoleWindowController))[0];
			InfoSpaceController = (InfoSpaceController) Resources.FindObjectsOfTypeAll(typeof(InfoSpaceController))[0];
		}

		void Start() {
			if(Environment == Environment.Cave)
				foreach (var flystick in Flysticks)
					flystick.GetComponent<LineRenderer>().enabled = ShowFlystickRays;
			
			if (!NetworkController.IsServer())
				return;
			InputProcessor input = GetEnvDependent((InputProcessor) new PCInputProcessor(PCConfig, PCInputBinding, this),
				 new CaveInputProcessor(CaveConfig, CaveInputBinding, this));
			binding = GetEnvDependent((InputBinding) PCInputBinding, CaveInputBinding);

			CaveInputBinding.SetPrimaryFlystick(0);
			binding.Init();
			SetBlockInput(true, InputBlockType.INFO_SPACE);
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

		public void SetBlockInput(bool block, InputBlockType blockType) {
			BlockType = block ? blockType : (InputBlockType?) null;
			binding.CallFieldsOfType<InputBlocker>(field => field.SetBlocked(block), field => IsEventBlocked(field, blockType));
		}

		private bool IsEventBlocked(FieldInfo field, InputBlockType blockType) {
			NotBlocked blockedAttribute = field.GetCustomAttribute<NotBlocked>();
			return blockedAttribute == null || blockedAttribute.NotBlockedTypes.Count > 0 && !blockedAttribute.NotBlockedTypes.Contains(blockType);
		}

		private T GetEnvDependent<T>(T pc, T cave) => Environment == Environment.Cave ? cave : pc;
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