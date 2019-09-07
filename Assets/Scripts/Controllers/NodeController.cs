using Model;
using Services;
using Services.DataFiles;
using Services.ObjectPool;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Controllers {
	public class NodeController : MonoBehaviour {
		public NodeColor[] NodeColors;
		public NodeSprites NodeSprites;

		public GraphPooledObject Nodes;

		public int NodeLoadedLimit;
		public bool LoadTestNodeSet;

		private GraphController graphController;

		#region Highlighted Node

		private Node highlightedNode;
		public Node HighlightedNode {
			get { return highlightedNode; }
			set {
				if (highlightedNode == value || NewNodeDisabled(value)) return;
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
				if (NewNodeDisabled(value)) return;
				if (selectedNode == value) {
					graphController.SwitchConnectionMode();
					return;
				}
				Node previousNode = selectedNode;
				selectedNode = value;
				UpdateNodeStates();
				graphController.GraphMode.Value = selectedNode != null ? GraphMode.NODE_TRAVERSE : GraphMode.FREE_FLIGHT;
				OnSelectedNodeChanged?.Invoke(previousNode, selectedNode);
			}
		}

		public Action<Node, Node> OnSelectedNodeChanged;

		#endregion

		private bool NewNodeDisabled(Node newVal) => newVal != null && newVal.State == NodeState.DISABLED;

		#region Node Loading

		private NodeLoader nodeLoader;

		public void LoadNode(uint id) {
			if (GraphController.Graph.IdNodeMap.ContainsKey(id)) return;
			Node node = nodeLoader.LoadNode(id);
			GraphController.Graph.IdNodeMap[id] = node;
			GameObject nodeObject = Nodes.Pool.Spawn();
			InitializeNode(node, ref nodeObject, Random.insideUnitSphere * graphController.WorldRadius);
			GraphController.Graph.NodeObjectMap[node] = nodeObject;
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
			}
			else {
				SetAllNodesAs(NodeState.DISABLED);
				SetNodeState(selectedNode, NodeState.SELECTED);
				foreach (var connection in connectionController.GetActiveNodeConnections()) {
					LoadNode(connection.ID);
					SetNodeState(connection, NodeState.ACTIVE);
				}
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

		public void ForceSetSelect(Node node) {
			SetNodeState(node, Model.NodeState.ACTIVE);
			SelectedNode = node;
		}
		#endregion

		#region Mono Behaviour

		private ConnectionController connectionController;

		void Awake() {
			graphController = GetComponent<GraphController>();
			connectionController = GetComponent<ConnectionController>();
		}

		private void Start() {
			Nodes.Pool = new GameObjectPool(Nodes.Prefab, Nodes.PreloadNumber, Nodes.PoolContainer);
			nodeLoader = new NodeLoader(LoadTestNodeSet ? "-test" : "");
			for (uint i = 0; i < Math.Min(NodeLoadedLimit, nodeLoader.GetNodeNumber()); i++) {
				LoadNode(i);
			}
			graphController.GraphMode.OnValueChanged += mode => {
				if (mode == GraphMode.FREE_FLIGHT) SelectedNode = null;
			};
			graphController.ConnectionMode.OnValueChanged += mode => UpdateNodeStates();
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