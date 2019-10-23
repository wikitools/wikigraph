using System.Collections;
using System.Linq;
using Controllers;
using Model;
using UnityEngine;
using UnityEngine.UI;

namespace Services.Nodes {
	public class NodeStateManager {
		private readonly Logger<NodeStateManager> logger = new Logger<NodeStateManager>();
		
		private NodeController controller;

		public NodeStateManager(NodeController controller) {
			this.controller = controller;
		}

		public void UpdateNodeStates() {
			SetAllNodesAs(DefaultState);
			if(controller.HighlightedNode != null)
				SetNodeState(controller.HighlightedNode, NodeState.HIGHLIGHTED);
			if (controller.GraphController.GraphMode.Value == GraphMode.NODE_TRAVERSE)
				SetNodeState(controller.SelectedNode, NodeState.SELECTED);
		}

		private void UpdateNodeObjectState(NodeState state, ref GameObject nodeObject) {
			nodeObject.GetComponent<SphereCollider>().enabled = state != NodeState.DISABLED;
			nodeObject.GetComponentInChildren<Image>().color = GetStateColor(state);
		}

		public NodeState DefaultState => controller.GraphController.GraphMode.Value == GraphMode.FREE_FLIGHT ? NodeState.ACTIVE : NodeState.DISABLED;

		private void SetAllNodesAs(NodeState state) {
			foreach (var node in GraphController.Graph.NodeObjectMap.Keys) {
				SetNodeState(node, state);
			}
		}

		public void SetConditionalNodeState(Node node, NodeState state) {
			if (controller.GraphController.GraphMode.Value == GraphMode.FREE_FLIGHT)
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
			var connection = GraphController.Graph.GetConnectionBetween(controller.SelectedNode, node);
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
			controller.StartCoroutine(AnimateScaleNodeImage(transform, to, controller.NodeScaleTime));
		}

		public void ScaleConnectionNodeImage(GameObject node, float from, float to) {
			ScaleNodeImage(node, from, to, controller.ConnectionNodeScaleTime);
		}

		public void ScaleNodeImage(Node node, float from, float to) {
			ScaleNodeImage(GraphController.Graph.NodeObjectMap[node], from, to, controller.NodeScaleTime);
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
				GameObject.Destroy(node.gameObject);
		}

		public void SetNodeColor(Node node, NodeState state) {
			var nodeObject = GraphController.Graph.NodeObjectMap[node];
			nodeObject.GetComponentInChildren<Image>().color = GetStateColor(state);
		}

		public Color GetStateColor(NodeState state) => controller.NodeColors.First(nodeColor => nodeColor.State == state).Color;

		public void ForceSetSelectedNode(Node node) {
			if(node != null && node.State != NodeState.SELECTED)
				SetNodeState(node, NodeState.ACTIVE);
			controller.SelectedNode = node;
		}
	}
}