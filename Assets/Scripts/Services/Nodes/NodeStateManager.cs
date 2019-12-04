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
				SetConditionalNodeState(controller.HighlightedNode, NodeState.HIGHLIGHTED);
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
			controller.NodeLoaderController.NodeChangeStateBehaviour(node, state);
			node.State = state;
			SetNodeObjectState(GraphController.Graph.NodeObjectMap[node], node.Type, state);
		}

		private void SetConnectionNodeState(Node node, NodeState state) {
			node.State = state;
			var connection = GraphController.Graph.GetConnectionBetween(controller.SelectedNode, node);
			if (connection == null || !GraphController.Graph.ConnectionNodes.ContainsKey(connection))
				return;
			SetNodeObjectState(GraphController.Graph.ConnectionNodes[connection], node.Type, state);
		}
		
		public void SetNodeObjectState(GameObject nodeObject, NodeType type, NodeState state) {
			var material = nodeObject.GetComponent<MeshRenderer>().material;
			material.mainTexture = GetStateSprite(type, state);
			nodeObject.GetComponent<SphereCollider>().enabled = state != NodeState.DISABLED;
			if(state == NodeState.DISABLED) {
				material.color = Random.ColorHSV(0.5f, 0.6f, 0.5f, 0.7f, 0.4f, 0.6f, 0.75f, 1f);
				nodeObject.transform.GetComponent<MeshRenderer>().sortingOrder = 1;
			} else {
				material.color = new Color(1, 1, 1, 1);
				nodeObject.transform.GetComponent<MeshRenderer>().sortingOrder = 10;
			}
		}
		
		public void SetNodeSprite(Node node, NodeState state) {
			var nodeObject = GraphController.Graph.NodeObjectMap[node];
			nodeObject.GetComponent<MeshRenderer>().material.mainTexture = GetStateSprite(node.Type, state);
		}
		
		public Texture GetStateSprite(NodeType type, NodeState state) => controller.NodeSprites.First(node => node.State == state && node.Type == type).Texture;

		public void ForceSetSelectedNode(Node node) {
			if(node != null && node.State != NodeState.SELECTED)
				SetNodeState(node, NodeState.ACTIVE);
			controller.SelectedNode = node;
		}
	}
}