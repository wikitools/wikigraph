using System;
using System.Collections;
using Controllers;
using Model;
using Services.Animations;
using Services.DataFiles;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Services.Nodes {
	public class NodeLoadManager: AnimationManager<NodeAnimation> {
		private NodeController controller;

		public NodeLoadManager(NodeController controller) : base(controller) {
			this.controller = controller;
			SetAnimationEndAction((node, animation) => AnimationEndAction(node, animation.Scale));
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
			nodeObject.layer = LayerMask.NameToLayer("Connection Node");
			ScaleConnectionNodeImage(nodeObject, 0, 1);
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
			ScaleConnectionNodeImage(nodeObject, -1, 0);
		}

		private void ScaleNodeImage(GameObject node, float from, float to, float time) {
			if (from >= 0)
				nodeImgTransform(node).localScale = Vector3.one * from;
			var function = AnimateScaleNodeImage(node, to, time);
			StartAnimation(node, new NodeAnimation(function, to));
		}

		public void ScaleConnectionNodeImage(GameObject node, float from, float to) {
			ScaleNodeImage(node, from, to, controller.ConnectionNodeScaleTime);
		}

		public void ScaleNodeImage(Node node, float from, float to) {
			ScaleNodeImage(GraphController.Graph.NodeObjectMap[node], from, to, controller.NodeScaleTime);
		}
		
		private RectTransform nodeImgTransform(GameObject node) => node.GetComponentInChildren<Image>().GetComponent<RectTransform>();

		private void AnimationEndAction(GameObject node, float scale) {
			if(scale == 0)
				controller.Nodes.Pool.Despawn(node);
			ActiveAnimations.Remove(node);
		}
		
		private IEnumerator AnimateScaleNodeImage(GameObject node, float scale, float time) {
			var transform = nodeImgTransform(node);
			float incAmount = (scale - transform.localScale.x) * Time.deltaTime / time;
			while (true) {
				if (Mathf.Abs(transform.localScale.x - scale) > Mathf.Abs(incAmount)) {
					transform.localScale = Vector3.one * (transform.localScale.x + incAmount);
					yield return null;
				} else {
					transform.localScale = Vector3.one * scale;
					break;
				}
			}
			AnimationEndAction(node, scale);
		}

		private void UpdateNodeObjectState(NodeState state, ref GameObject nodeObject) {
			nodeObject.GetComponent<SphereCollider>().enabled = state != NodeState.DISABLED;
			nodeObject.GetComponentInChildren<Image>().color = controller.NodeStateManager.GetStateColor(state);
		}
		
	}
}