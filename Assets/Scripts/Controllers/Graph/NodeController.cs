using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Model;
using Services;
using Services.DataFiles;
using Services.Nodes;
using Services.ObjectPool;
using UnityEditor;
using UnityEngine;

namespace Controllers {
	public class NodeController : MonoBehaviour {
		
		public NodeSprite[] NodeSprites;

		public GraphPooledObject Nodes;

		public int NodeLoadedLimit;
		[HideInInspector]
		public string DataPack;

		[Range(.05f, 2f)]
		public float NodeScaleTime = .8f;
		[Range(.05f, 2f)]
		public float ConnectionNodeScaleTime = 0.4f;

		public Action<Node, Vector3> OnNodeLoaded;
		public Action<Node> OnNodeUnloaded;
		public Action OnNodeLoadSessionEnded;

		public Action<Node, Node> OnSelectedNodeChanged;
		public Action<Node, Node> OnHighlightedNodeChanged;

		public NodeLoadManager NodeLoadManager;
		public NodeStateManager NodeStateManager;
		
		#region Highlighted Node

		private Node highlightedNode;

		public Node HighlightedNode {
			get { return highlightedNode; }
			set {
				if (highlightedNode == value || SelectedNode == null && value != null && value.State == NodeState.DISABLED) return;
				if (highlightedNode != null) {
					if (highlightedNode.State != NodeState.SELECTED)
						NodeStateManager.SetConditionalNodeState(highlightedNode, NodeState.ACTIVE);
					else if(inputController.Environment == Environment.Cave)
						NodeStateManager.SetNodeSprite(highlightedNode, NodeState.SELECTED);
				}
				Node previousNode = highlightedNode;
				highlightedNode = value;
				if (highlightedNode != null) {
					if (highlightedNode.State != NodeState.SELECTED)
						NodeStateManager.SetConditionalNodeState(highlightedNode, NodeState.HIGHLIGHTED);
					else if(inputController.Environment == Environment.Cave)
						NodeStateManager.SetNodeSprite(highlightedNode, NodeState.HIGHLIGHTED);
				}
				OnHighlightedNodeChanged?.Invoke(previousNode, highlightedNode);
			}
		}

		#endregion

		#region Selected Node

		private Node selectedNode;

		public Node SelectedNode {
			get { return selectedNode; }
			set {
				if (selectedNode == value) {
					if (inputController.Environment == Environment.Cave)
						GraphController.ConnectionMode.Value = GraphController.GetSwitchedConnectionMode();
					return;
				}
				Node previousNode = selectedNode;
				selectedNode = value;
				GraphController.GraphMode.Value = selectedNode != null ? GraphMode.NODE_TRAVERSE : GraphMode.FREE_FLIGHT;
				NodeStateManager.UpdateNodeStates();
				if(previousNode != null)
					NodeLoadManager.AnimScaleNodeSize(previousNode, -1, NodeLoadManager.GetNodeTypeScale(previousNode.Type));
				if(selectedNode != null)
					NodeLoadManager.AnimScaleNodeSize(selectedNode, -1, 3f * NodeLoadManager.GetNodeTypeScale(selectedNode.Type));
				OnSelectedNodeChanged?.Invoke(previousNode, selectedNode);
			}
		}

		public bool IsNodeInteractable(int layer, string id) {
			bool modeCondition = id == "" || (GraphController.GraphMode.Value == GraphMode.FREE_FLIGHT ? layer == LayerMask.NameToLayer("Node")
				                     : SelectedNode.ID.ToString() == id || layer == LayerMask.NameToLayer("Connection Node"));
			return (highlightedNode != null ? highlightedNode.ID.ToString() : "") != id && modeCondition;
		}

		#endregion

		#region Mono Behaviour

		public GraphController GraphController { get; private set; }
		private NetworkController networkController;
		private InputController inputController;

		void Awake() {
			GraphController = GetComponent<GraphController>();
			networkController = GetComponent<NetworkController>();
			inputController = GetComponent<InputController>();
		}

		private void Start() {
			NodeLoadManager = new NodeLoadManager(this);
			NodeStateManager = new NodeStateManager(this);
			Nodes.Pool = new GameObjectPool(Nodes.Prefab, Nodes.PreloadNumber, Nodes.PoolContainer);

			if (networkController.IsServer()) {
				for (uint i = 0; i < Math.Min(NodeLoadedLimit, NodeLoadManager.NodeLoader.GetNodeNumber()); i++)
					NodeLoadManager.LoadNode(i);
				OnNodeLoadSessionEnded?.Invoke();
				GraphController.GraphMode.OnValueChanged += mode => {
					if (mode == GraphMode.FREE_FLIGHT)
						networkController.SetSelectedNode((Node) null);
				};
			}
		}

		private void OnDestroy() {
			NodeLoadManager.NodeLoader.Dispose();
		}

		#endregion
	}

	[Serializable]
	public class NodeSprite {
		public NodeType Type;
		public NodeState State;
		public Sprite Sprite;
	}

#if UNITY_EDITOR
	[CustomEditor(typeof(NodeController))]
	public class NodeInspector: Editor {
		private List<string> dataPacks = new List<string>();
		
		private SerializedProperty dataPack;
		private SerializedObject script;
		private GUIStyle labelStyle = new GUIStyle();
		
		private void OnEnable() {
			script = new SerializedObject(target);
			dataPack = script.FindProperty("DataPack");
			ScanDataPacks();
			labelStyle.alignment = TextAnchor.MiddleCenter;
		}

		public override void OnInspectorGUI() {
			base.OnInspectorGUI();
			if (EditorApplication.isPlaying) 
				GUI.enabled = false;
			
			if (dataPacks.Count == 0)
				EditorGUILayout.LabelField("No Data Packs found", labelStyle);
			else {
				var index = dataPacks.IndexOf(dataPack.stringValue);
				int selectedDataPack = EditorGUILayout.Popup("Data Pack", Mathf.Clamp(index, 0, dataPacks.Count), dataPacks.ToArray());
				dataPack.stringValue = dataPacks[selectedDataPack];
			}
			if (GUILayout.Button("Reload Data Packs"))
				ScanDataPacks();
			GUI.enabled = true;
			script.ApplyModifiedProperties();
		}

		private void ScanDataPacks() {
			dataPacks = Directory.GetDirectories(DataFileReader.DATA_FILE_PATH).Select(Path.GetFileName).ToList();
		}
	}
#endif
}