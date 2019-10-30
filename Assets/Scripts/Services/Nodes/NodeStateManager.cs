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
			nodeObject.GetComponentInChildren<Image>().color = GetStateColor(state);
			nodeObject.GetComponent<SphereCollider>().enabled = state != NodeState.DISABLED;
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