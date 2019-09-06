using System;
using System.Linq;
using Model;
using Services;
using Services.DataFiles;
using Services.ObjectPool;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Controllers {
	public class NodeController: MonoBehaviour {
		public NodeColor[] NodeColors;
		public NodeSprites NodeSprites;
		
		public GraphPooledObject Nodes;
		
		public int NodeLoadedLimit;
		public bool LoadTestNodeSet;

		public Action<Node, Vector3> OnNodeLoaded;
		public Action<Node> OnNodeUnloaded;
		public Action OnNodeLoadSessionEnded;
		
		#region Highlighted Node
		
		private Node highlightedNode;
		public Node HighlightedNode {
			get { return highlightedNode; }
			set {
				if(highlightedNode == value || NewNodeDisabled(value)) return;
				if (highlightedNode != null && highlightedNode.State != NodeState.SELECTED) 
					SetNodeState(highlightedNode, NodeState.ACTIVE);
				highlightedNode = value;
				if (highlightedNode != null && highlightedNode.State != NodeState.SELECTED) 
					SetNodeState(highlightedNode, NodeState.HIGHLIGHTED);
			}
		}
		
		#endregion
		
		#region Selected Node

		private Node selectedNode;
		public Node SelectedNode {
			get { return selectedNode; }
			set {
				if(NewNodeDisabled(value)) return;
				if (selectedNode == value) {
					if(inputController.Environment == Environment.Cave)
						graphController.SwitchConnectionMode();
					return;
				}
				selectedNode = value;
				UpdateNodeStates();
				graphController.GraphMode.Value = selectedNode != null ? GraphMode.NODE_TRAVERSE : GraphMode.FREE_FLIGHT;
				OnSelectedNodeChanged?.Invoke(selectedNode);
				OnNodeLoadSessionEnded?.Invoke();//can trigger loading of unloaded connected nodes TODO move once we have a node loader
			}
		}

		public Action<Node> OnSelectedNodeChanged;
		
		#endregion

		private bool NewNodeDisabled(Node newVal) => newVal != null && newVal.State == NodeState.DISABLED;

		#region Node Loading
		
		private NodeLoader nodeLoader;

		public Node LoadNode(uint id) {
			return LoadNode(id, Random.insideUnitSphere * graphController.WorldRadius);
		}

		public Node LoadNode(uint id, Vector3 position) {
			if(GraphController.Graph.IdNodeMap.ContainsKey(id)) 
				return GraphController.Graph.IdNodeMap[id];
			Node node = nodeLoader.LoadNode(id);
			GraphController.Graph.IdNodeMap[id] = node;
			GameObject nodeObject = Nodes.Pool.Spawn();
			InitializeNode(node, ref nodeObject, position);
			GraphController.Graph.NodeObjectMap[node] = nodeObject;
			
			OnNodeLoaded?.Invoke(node, position);
			return node;
		}

		public void InitializeNode(Node model, ref GameObject nodeObject, Vector3 position) {
			nodeObject.transform.parent = Nodes.Container.transform;
			nodeObject.transform.position = position;
			nodeObject.GetComponentInChildren<Text>().text = model.Title;
			var nodeImage = nodeObject.GetComponentInChildren<Image>();
			nodeImage.sprite = model.Type == NodeType.ARTICLE ? NodeSprites.Article : NodeSprites.Category;
			nodeImage.color = NodeColors.First(node => node.State == NodeState.ACTIVE).Color;
			nodeObject.name = model.ID.ToString();
		}
		
		#endregion

		#region Node States

		private void UpdateNodeStates() {
			if (selectedNode == null) {
				SetAllNodesAs(NodeState.ACTIVE);
			} else {
				SetAllNodesAs(NodeState.DISABLED);
				SetNodeState(selectedNode, NodeState.SELECTED);
			}
		}

		private void SetAllNodesAs(NodeState state) {
			foreach (var node in GraphController.Graph.NodeObjectMap.Keys) {
				SetNodeState(node, state);
			}
		}

		private void SetNodeState(Node node, NodeState state) {
			node.State = state;
			var nodeObject = GraphController.Graph.NodeObjectMap[node];
			nodeObject.GetComponentInChildren<Text>().enabled = node.State == NodeState.SELECTED || node.State == NodeState.HIGHLIGHTED;
			nodeObject.GetComponentInChildren<Image>().color = NodeColors.First(nodeColor => nodeColor.State == state).Color;
		}

		#endregion
		
		#region Mono Behaviour
		
		private ConnectionController connectionController;
		private GraphController graphController;
		private NetworkController networkController;
		private InputController inputController;
		 
		void Awake() {
			graphController = GetComponent<GraphController>();
			connectionController = GetComponent<ConnectionController>();
			networkController = GetComponent<NetworkController>();
			inputController = GetComponent<InputController>();
		}

		private void Start() {
			Nodes.Pool = new GameObjectPool(Nodes.Prefab, Nodes.PreloadNumber, Nodes.PoolContainer);
			nodeLoader = new NodeLoader(LoadTestNodeSet ? "-test" : "");
			
			if (networkController.IsServer()) {
				for (uint i = 0; i < Math.Min(NodeLoadedLimit, nodeLoader.GetNodeNumber()); i++) {
					LoadNode(i);
				}
				OnNodeLoadSessionEnded?.Invoke();
			}
			graphController.GraphMode.OnValueChanged += mode => {
				if (mode == GraphMode.FREE_FLIGHT) SelectedNode = null;
			};
			graphController.ConnectionMode.OnValueChanged += mode => UpdateNodeStates();

			connectionController.OnConnectionLoaded += connection => SetNodeState(connection.Item2, NodeState.ACTIVE);
			connectionController.OnConnectionUnloaded += connection => SetNodeState(connection.Item2, NodeState.DISABLED);
		}

		private void OnDestroy() {
			nodeLoader.Dispose();
		}
		
		#endregion
	}
	
	[Serializable]
	public class NodeColor {
		public NodeState State;
		public Color Color;
	}
	
	[Serializable]
	public class NodeSprites {
		public Sprite Article;
		public Sprite Category;
	}
}