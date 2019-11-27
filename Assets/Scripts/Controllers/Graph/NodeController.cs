using System;
using Model;
using Services;
using Services.Nodes;
using Services.ObjectPool;
using UnityEngine;

namespace Controllers {
	public class NodeController : MonoBehaviour {
		public NodeSprite[] NodeSprites;

		public GraphPooledObject Nodes;
		public Material NodeMaterial;

		[HideInInspector] public string DataPack;
		[HideInInspector] public string DataPackDate;

		[Range(.05f, 2f)] public float NodeScaleTime = .8f;
		[Range(.05f, 2f)] public float ConnectionNodeScaleTime = 0.4f;

		public Action<Node, Vector3> OnNodeLoaded;
		public Action<Node> OnNodeUnloaded;
		public Action OnNodeLoadSessionEnded;

		public Action<Node, Node> OnSelectedNodeChanged;
		public Action<Node, Node> OnHighlightedNodeChanged;

		public NodeLoadManager NodeLoadManager;
		public NodeStateManager NodeStateManager;
		public NodeLoaderController NodeLoaderController { get; private set; }

		#region Highlighted Node

		private Node highlightedNode;

		public Node HighlightedNode {
			get { return highlightedNode; }
			set {
				if (highlightedNode == value || SelectedNode == null && value != null && value.State == NodeState.DISABLED) return;
				if (highlightedNode != null) {
					if (highlightedNode.State != NodeState.SELECTED)
						NodeStateManager.SetConditionalNodeState(highlightedNode, NodeState.ACTIVE);
					else if (inputController.Environment == Environment.Cave)
						NodeStateManager.SetNodeSprite(highlightedNode, NodeState.SELECTED);
				}
				Node previousNode = highlightedNode;
				highlightedNode = value;
				if (highlightedNode != null) {
					if (highlightedNode.State != NodeState.SELECTED)
						NodeStateManager.SetConditionalNodeState(highlightedNode, NodeState.HIGHLIGHTED);
					else if (inputController.Environment == Environment.Cave)
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
				if (previousNode != null)
					NodeLoadManager.AnimScaleNodeSize(previousNode, -1, NodeLoadManager.GetNodeTypeScale(previousNode.Type));
				if (selectedNode != null)
					NodeLoadManager.AnimScaleNodeSize(selectedNode, -1, 3f * NodeLoadManager.GetNodeTypeScale(selectedNode.Type));
				OnSelectedNodeChanged?.Invoke(previousNode, selectedNode);
			}
		}

		public bool IsNodeInteractable(int layer, string id) {
			bool modeCondition = id == null || (GraphController.GraphMode.Value == GraphMode.FREE_FLIGHT
				                     ? layer == LayerMask.NameToLayer("Node")
				                     : SelectedNode.ID.ToString() == id || layer == LayerMask.NameToLayer("Connection Node"));
			return (HighlightedNode != null ? HighlightedNode.ID.ToString() : null) != id && modeCondition;
		}

		#endregion

		private Mesh GenerateNodePlane() {
			return new Mesh {
				vertices = new[] {new Vector3(-.5f, -.5f, 0), new Vector3(.5f, -.5f, 0), new Vector3(-.5f, .5f, 0), new Vector3(.5f, .5f, 0)},
				triangles = new [] {0, 2, 1, 2, 3, 1},
				normals = new[] {Vector3.forward, Vector3.forward, Vector3.forward, Vector3.forward},
				uv = new[] {new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1)},
				bounds = new Bounds(Vector3.zero, Vector3.one)
			};
		}

		#region Mono Behaviour

		public GraphController GraphController { get; private set; }
		public NetworkController NetworkController { get; private set; }
		private InputController inputController;

		void Awake() {
			GraphController = GetComponent<GraphController>();
			NetworkController = GetComponent<NetworkController>();
			inputController = GetComponent<InputController>();
			NodeLoadManager = new NodeLoadManager(this);
			NodeLoaderController = GetComponent<NodeLoaderController>();
			
			Nodes.Prefab.GetComponent<MeshFilter>().sharedMesh = GenerateNodePlane();
		}

		private void Start() {
			NodeStateManager = new NodeStateManager(this);
			Nodes.Pool = new GameObjectPool(Nodes.Prefab, Nodes.PreloadNumber, Nodes.PoolContainer);
		}

		private void Update() {
			var eyes = inputController.Eyes.position;
			Shader.SetGlobalVector("_FaceObject", new Vector4(eyes.x, eyes.y, eyes.z));
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
		public Texture Texture;
	}
}