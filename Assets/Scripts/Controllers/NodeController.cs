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
		
		private NodeLoader nodeLoader;

		private GraphController graphController;
		
		private Node highlightedNode;
		public Node HighlightedNode {
			get { return highlightedNode; }
			set {
				if(IgnoreNodeValueChange(highlightedNode, value)) return;
				if (highlightedNode != null && highlightedNode.State != NodeState.SELECTED) 
					SetNodeState(highlightedNode, NodeState.ACTIVE);
				highlightedNode = value;
				if (highlightedNode != null && highlightedNode.State != NodeState.SELECTED) 
					SetNodeState(highlightedNode, NodeState.HIGHLIGHTED);
			}
		}

		private Node selectedNode;
		public Node SelectedNode {
			get { return selectedNode; }
			set {
				if(IgnoreNodeValueChange(selectedNode, value)) return;
				selectedNode = value;
				OnNodeSelectChanged(selectedNode);
				graphController.GraphMode = selectedNode != null ? GraphMode.NODE_TRAVERSE : GraphMode.FREE_FLIGHT;
				OnSelectedNodeChanged?.Invoke(selectedNode);
			}
		}

		private bool IgnoreNodeValueChange(Node oldVal, Node newVal) => oldVal == newVal || newVal != null && newVal.State == NodeState.DISABLED;

		public Action<Node> OnSelectedNodeChanged;

		public void LoadNode(uint id) {
			if(GraphController.Graph.IdNodeMap.ContainsKey(id)) return;
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

		private void OnNodeSelectChanged(Node selectedNode) {
			if (selectedNode == null) {
				SetAllNodesAs(NodeState.ACTIVE);
			} else {
				SetAllNodesAs(NodeState.DISABLED);
				var modNode = selectedNode;
				SetNodeState(modNode, NodeState.SELECTED);
				foreach (var childId in modNode.Children) {
					LoadNode(childId);
					var child = GraphController.Graph.IdNodeMap[childId];
					SetNodeState(child, NodeState.ACTIVE);
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
			nodeObject.GetComponentInChildren<Text>().enabled = node.State == NodeState.SELECTED;
			nodeObject.GetComponentInChildren<Image>().color = NodeColors.First(nodeColor => nodeColor.State == state).Color;
		}
		
		void Awake() {
			graphController = GetComponent<GraphController>();
		}

		private void Start() {
			Nodes.Pool = new GameObjectPool(Nodes.Prefab, Nodes.PreloadNumber, Nodes.PoolContainer);
			nodeLoader = new NodeLoader(LoadTestNodeSet ? "-test" : "");
			for (uint i = 0; i < Math.Min(NodeLoadedLimit, nodeLoader.GetNodeNumber()); i++) {
				LoadNode(i);
			}
		}

		private void OnDestroy() {
			nodeLoader.Dispose();
		}
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