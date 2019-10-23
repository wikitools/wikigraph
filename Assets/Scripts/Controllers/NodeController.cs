using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Model;
using Model.Connection;
using Services;
using Services.DataFiles;
using Services.ObjectPool;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Controllers {
	public class NodeController : MonoBehaviour {
		private readonly Logger<NodeController> logger = new Logger<NodeController>();
		
		public NodeColor[] NodeColors;
		public NodeSprites NodeSprites;

		public GraphPooledObject Nodes;

		public int NodeLoadedLimit;
		public bool LoadTestNodeSet;

		[Range(.1f, 2f)]
		public float NodeScaleTime = .5f;
		[Range(.1f, 2f)]
		public float ConnectionNodeScaleTime = 0.2f;

		public Action<Node, Vector3> OnNodeLoaded;
		public Action<Node> OnNodeUnloaded;
		public Action OnNodeLoadSessionEnded;

		public Action<Node, Node> OnSelectedNodeChanged;
		public Action<Node, Node> OnHighlightedNodeChanged;
		
		#region Highlighted Node

		private Node highlightedNode;

		public Node HighlightedNode {
			get { return highlightedNode; }
			set {
				if (highlightedNode == value || SelectedNode == null && value != null && value.State == NodeState.DISABLED) return;
				if (highlightedNode != null) {
					if (highlightedNode.State != NodeState.SELECTED)
						SetConditionalNodeState(highlightedNode, NodeState.ACTIVE);
					else if(inputController.Environment == Environment.Cave)
						SetNodeColor(highlightedNode, NodeState.SELECTED);
				}
				Node previousNode = highlightedNode;
				highlightedNode = value;
				if (highlightedNode != null) {
					if (highlightedNode.State != NodeState.SELECTED)
						SetConditionalNodeState(highlightedNode, NodeState.HIGHLIGHTED);
					else if(inputController.Environment == Environment.Cave)
						SetNodeColor(highlightedNode, NodeState.HIGHLIGHTED);
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
						graphController.ConnectionMode.Value = graphController.GetSwitchedConnectionMode();
					return;
				}
				if (selectedNode != null && value != null && !selectedNode.GetConnections(graphController.ConnectionMode.Value).Contains(value.ID)) return;
				Node previousNode = selectedNode;
				selectedNode = value;
				graphController.GraphMode.Value = selectedNode != null ? GraphMode.NODE_TRAVERSE : GraphMode.FREE_FLIGHT;
				UpdateNodeStates();
				if(previousNode != null)
					ScaleNodeImage(previousNode, -1, 1);
				if(selectedNode != null)
					ScaleNodeImage(selectedNode, -1, 3f);
				OnSelectedNodeChanged?.Invoke(previousNode, selectedNode);
			}
		}

		public bool IsNodeInteractable(int layer, string id) {
			bool modeCondition = id == "" || (graphController.GraphMode.Value == GraphMode.FREE_FLIGHT ? layer == LayerMask.NameToLayer("Node")
				                     : SelectedNode.ID.ToString() == id || layer == LayerMask.NameToLayer("Connection Node"));
			return (highlightedNode != null ? highlightedNode.ID.ToString() : "") != id && modeCondition;
		}

		#endregion

		#region Node Loading

		private NodeLoader nodeLoader;

		public Node LoadNode(uint id, bool skipScaling = false) {
			return LoadNode(id, Random.insideUnitSphere * graphController.WorldRadius, skipScaling);
		}

		public Node LoadNode(uint id, Vector3 position, bool skipScaling = false) {
			if (GraphController.Graph.IdNodeMap.ContainsKey(id))
				return GraphController.Graph.IdNodeMap[id];
			Node node = nodeLoader.LoadNode(id);
			node.State = DefaultState;
			GraphController.Graph.IdNodeMap[id] = node;
			GameObject nodeObject = Nodes.Pool.Spawn();
			InitializeNode(node, ref nodeObject, position);
			GraphController.Graph.NodeObjectMap[node] = nodeObject;
			if(!skipScaling)
				ScaleNodeImage(node, 0, 1);

			OnNodeLoaded?.Invoke(node, position);
			return node;
		}

		public GameObject LoadConnectionNode(Node model, Vector3 position) {
			GameObject nodeObject = Nodes.Pool.Spawn();
			InitializeNode(model, ref nodeObject, position);
			UpdateNodeObjectState(NodeState.ACTIVE, ref nodeObject);
			nodeObject.GetComponent<SphereCollider>().radius = 1;
			nodeObject.layer = LayerMask.NameToLayer("Connection Node");
			ScaleConnectionNodeImage(nodeObject, 0, 1);
			return nodeObject;
		}

		public void InitializeNode(Node model, ref GameObject nodeObject, Vector3 position) {
			nodeObject.transform.parent = Nodes.Container.transform;
			nodeObject.transform.position = position;
			nodeObject.GetComponentInChildren<Text>().text = model.Title;
			UpdateNodeObjectState(model.State, ref nodeObject);
			var nodeImage = nodeObject.GetComponentInChildren<Image>();
			nodeImage.sprite = model.Type == NodeType.ARTICLE ? NodeSprites.Article : NodeSprites.Category;
			nodeObject.name = model.ID.ToString();
		}

		private void UpdateNodeObjectState(NodeState state, ref GameObject nodeObject) {
			nodeObject.GetComponent<SphereCollider>().enabled = state != NodeState.DISABLED;
			nodeObject.GetComponentInChildren<Image>().color = GetStateColor(state);
		}

		#endregion

		#region Node States

		private void UpdateNodeStates() {
			SetAllNodesAs(DefaultState);
			if(HighlightedNode != null)
				SetNodeState(HighlightedNode, NodeState.HIGHLIGHTED);
			if (graphController.GraphMode.Value == GraphMode.NODE_TRAVERSE)
				SetNodeState(selectedNode, NodeState.SELECTED);
		}

		private NodeState DefaultState => graphController.GraphMode.Value == GraphMode.FREE_FLIGHT ? NodeState.ACTIVE : NodeState.DISABLED;

		private void SetAllNodesAs(NodeState state) {
			foreach (var node in GraphController.Graph.NodeObjectMap.Keys) {
				SetNodeState(node, state);
			}
		}

		private void SetConditionalNodeState(Node node, NodeState state) {
			if (graphController.GraphMode.Value == GraphMode.FREE_FLIGHT)
				SetNodeState(node, state);
			else
				SetConnectionNodeState(node, state);
		}

		private void SetNodeState(Node node, NodeState state) {
			node.State = state;
			SetNodeObjectState(GraphController.Graph.NodeObjectMap[node], state);
		}

		private void SetConnectionNodeState(Node node, NodeState state) {
			node.State = state;
			var connection = GraphController.Graph.GetConnectionBetween(SelectedNode, node);
			if (connection == null || !GraphController.Graph.ConnectionNodes.ContainsKey(connection)) {
				logger.Error("No connection for node found.");
				return;
			}
			SetNodeObjectState(GraphController.Graph.ConnectionNodes[connection], state);
		}

		private void SetNodeObjectState(GameObject nodeObject, NodeState state) {
			nodeObject.GetComponentInChildren<Text>().enabled = state == NodeState.SELECTED || state == NodeState.HIGHLIGHTED;
			nodeObject.GetComponentInChildren<Image>().color = GetStateColor(state);
			nodeObject.GetComponent<SphereCollider>().enabled = state != NodeState.DISABLED;
		}

		private void ScaleNodeImage(GameObject node, float from, float to, float time) {
			var transform = node.GetComponentInChildren<Image>().GetComponent<RectTransform>();
			if (from >= 0)
				transform.localScale = Vector3.one * from;
			StartCoroutine(AnimateScaleNodeImage(transform, to, NodeScaleTime));
		}

		private void ScaleConnectionNodeImage(GameObject node, float from, float to) {
			ScaleNodeImage(node, from, to, ConnectionNodeScaleTime);
		}

		private void ScaleNodeImage(Node node, float from, float to) {
			ScaleNodeImage(GraphController.Graph.NodeObjectMap[node], from, to, NodeScaleTime);
		}

		private IEnumerator AnimateScaleNodeImage(RectTransform node, float scale, float time) {
			float incAmount = (scale - node.localScale.x) / 100f / time;
			while (true) {
				if (Mathf.Abs(node.localScale.x - scale) > Mathf.Abs(incAmount)) {
					node.localScale = Vector3.one * (node.localScale.x + incAmount);
					yield return new WaitForSeconds(.01f);
				} else {
					node.localScale = Vector3.one * scale;
					break;
				}
			}
			if(scale == 0)
				Destroy(node.gameObject);
		}

		private void SetNodeColor(Node node, NodeState state) {
			var nodeObject = GraphController.Graph.NodeObjectMap[node];
			nodeObject.GetComponentInChildren<Image>().color = GetStateColor(state);
		}
		
		private Color GetStateColor(NodeState state) => NodeColors.First(nodeColor => nodeColor.State == state).Color;

		public void ForceSetSelectedNode(Node node) {
			if(node != null && node.State != NodeState.SELECTED)
				SetNodeState(node, NodeState.ACTIVE);
			SelectedNode = node;
		}

		#endregion

		#region Mono Behaviour

		private GraphController graphController;
		private NetworkController networkController;
		private InputController inputController;

		void Awake() {
			graphController = GetComponent<GraphController>();
			networkController = GetComponent<NetworkController>();
			inputController = GetComponent<InputController>();
		}

		private void Start() {
			Nodes.Pool = new GameObjectPool(Nodes.Prefab, Nodes.PreloadNumber, Nodes.PoolContainer);
			nodeLoader = new NodeLoader(LoadTestNodeSet ? "-test" : "");

			if (networkController.IsServer()) {
				for (uint i = 0; i < Math.Min(NodeLoadedLimit, nodeLoader.GetNodeNumber()); i++)
					LoadNode(i, true);
				OnNodeLoadSessionEnded?.Invoke();
				graphController.GraphMode.OnValueChanged += mode => {
					if (mode == GraphMode.FREE_FLIGHT)
						networkController.SetSelectedNode("");
				};
			}
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