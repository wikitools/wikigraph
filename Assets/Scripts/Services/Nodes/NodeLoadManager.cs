using System;
using System.Collections;
using Controllers;
using Model;
using Services.DataFiles;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Services.Nodes {
	public class NodeLoadManager {
		private NodeController controller;

		public NodeLoadManager(NodeController controller) {
			this.controller = controller;
			NodeLoader = new NodeLoader(controller.LoadTestNodeSet ? "-test" : "");
		}

		public NodeLoader NodeLoader;

		public Node LoadNode(uint id, bool skipScaling = false) {
			return LoadNode(id, Random.insideUnitSphere * controller.GraphController.WorldRadius, skipScaling);
		}

		public Node LoadNode(uint id, Vector3 position, bool skipScaling = false) {
			if (GraphController.Graph.IdNodeMap.ContainsKey(id))
				return GraphController.Graph.IdNodeMap[id];
			Node node = NodeLoader.LoadNode(id);
			node.State = controller.NodeStateManager.DefaultState;
			GraphController.Graph.IdNodeMap[id] = node;
			GameObject nodeObject = controller.Nodes.Pool.Spawn();
			InitializeNode(node, ref nodeObject, position);
			GraphController.Graph.NodeObjectMap[node] = nodeObject;

			controller.OnNodeLoaded?.Invoke(node, position);
			return node;
		}

		public GameObject LoadConnectionNode(Node model, Vector3 position) {
			GameObject nodeObject = controller.Nodes.Pool.Spawn();
			InitializeNode(model, ref nodeObject, position);
			UpdateNodeObjectState(NodeState.ACTIVE, ref nodeObject);
			nodeObject.GetComponent<SphereCollider>().radius = 1;
			nodeObject.layer = LayerMask.NameToLayer("Connection Node");
			return nodeObject;
		}

		public void InitializeNode(Node model, ref GameObject nodeObject, Vector3 position) {
			nodeObject.transform.parent = controller.Nodes.Container.transform;
			nodeObject.transform.position = position;
			UpdateNodeObjectState(model.State, ref nodeObject);
			var nodeImage = nodeObject.GetComponentInChildren<Image>();
			nodeImage.sprite = model.Type == NodeType.ARTICLE ? controller.NodeSprites.Article : controller.NodeSprites.Category;
			nodeObject.name = model.ID.ToString();
		}

		public void UnloadConnectionNode(Model.Connection.Connection connection) {
			var nodeObject = GraphController.Graph.ConnectionNodes[connection];
			GraphController.Graph.ConnectionNodes.Remove(connection);
			controller.Nodes.Pool.Despawn(nodeObject);
		}
		
		private IEnumerator ExecuteAfter(Action action, float time) {
			yield return new WaitForSeconds(time);
			action();
		}

		private void UpdateNodeObjectState(NodeState state, ref GameObject nodeObject) {
			nodeObject.GetComponent<SphereCollider>().enabled = state != NodeState.DISABLED;
			nodeObject.GetComponentInChildren<Image>().color = controller.NodeStateManager.GetStateColor(state);
		}
		
	}
}