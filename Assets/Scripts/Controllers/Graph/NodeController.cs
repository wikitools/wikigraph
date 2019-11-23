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
		
		[HideInInspector]
		public string DataPack;
		[HideInInspector]
		public string DataPackDate;

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
		public NodeLoaderController NodeLoaderController;
		
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
			NodeLoaderController = GetComponent<NodeLoaderController>();
		}

		private void Start() {
			NodeLoadManager = new NodeLoadManager(this);
			NodeStateManager = new NodeStateManager(this);
			Nodes.Pool = new GameObjectPool(Nodes.Prefab, Nodes.PreloadNumber, Nodes.PoolContainer);

			if (networkController.IsServer()) {
				for (uint i = 0; i < Math.Min(NodeLoaderController.nodeStartingAmount, NodeLoadManager.NodeLoader.GetNodeNumber()); i++) {
					NodeLoadManager.LoadNode(i);
					NodeLoaderController.AddLowPriorityNode(i);
				}
				OnNodeLoadSessionEnded?.Invoke();
				GraphController.GraphMode.OnValueChanged += mode => {
					if (mode == GraphMode.FREE_FLIGHT)
						networkController.SetSelectedNode((Node) null);
				};
			}
		}

		private void Update() {
			var eyes = inputController.Eyes.position;
			NodeMaterial.SetVector("_FaceObject", new Vector4(eyes.x, eyes.y, eyes.z));
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
}