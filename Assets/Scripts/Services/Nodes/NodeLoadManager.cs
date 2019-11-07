using System;
using System.Collections;
using System.Linq;
using Controllers;
using Model;
using Services.Animations;
using Services.DataFiles;
using UnityEngine;
using UnityEngine.UI;
using Object = System.Object;
using Random = UnityEngine.Random;

namespace Services.Nodes {
	public class NodeLoadManager: AnimationManager<NodeAnimation> {
		private const float CATEGORY_SCALE = 1.25F;
		private NodeController controller;

		public NodeLoadManager(NodeController controller) : base(controller) {
			this.controller = controller;
			SetAnimationEndAction((node, animation) => AnimationEndAction(node, animation.Scale));
			NodeLoader = new NodeLoader(controller.DataPack);
		}

		public NodeLoader NodeLoader;

		public Node LoadNode(uint id) {
			return LoadNode(id, Random.insideUnitSphere * controller.GraphController.WorldRadius);
		}

		public Node LoadNode(uint id, Vector3 position) {
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
			UpdateNodeObjectState(model.Type, NodeState.ACTIVE, ref nodeObject);
			nodeObject.layer = LayerMask.NameToLayer("Connection Node");
			AnimScaleConnectionNodeSize(nodeObject, 0, GetNodeTypeScale(model.Type));
			return nodeObject;
		}

		public void InitializeNode(Node model, ref GameObject nodeObject, Vector3 position) {
			nodeObject.transform.parent = controller.Nodes.Container.transform;
			nodeObject.transform.position = position;
			UpdateNodeObjectState(model.Type, model.State, ref nodeObject);
			var nodeImage = nodeObject.GetComponentInChildren<Image>();
			nodeImage.sprite = controller.NodeSprites.First(node => node.Type == model.Type && node.State == model.State).Sprite;
			nodeObject.name = model.ID.ToString();
			ScaleNodeSize(nodeObject, GetNodeTypeScale(model.Type));
		}

		public void UnloadConnectionNode(Model.Connection.Connection connection) {
			var nodeObject = GraphController.Graph.ConnectionNodes[connection];
			GraphController.Graph.ConnectionNodes.Remove(connection);
			AnimScaleConnectionNodeSize(nodeObject, -1, 0);
		}

		private void AnimScaleNodeSize(GameObject node, float from, float to, float time) {
			if (from >= 0)
				ScaleNodeSize(node, from);
			var function = AnimateNodeSize(node, to, time);
			StartAnimation(node, new NodeAnimation(function, to));
		}

		public void AnimScaleConnectionNodeSize(GameObject node, float from, float to) {
			AnimScaleNodeSize(node, from, to, controller.ConnectionNodeScaleTime);
		}

		public void AnimScaleNodeSize(Node node, float from, float to) {
			AnimScaleNodeSize(GraphController.Graph.NodeObjectMap[node], from, to, controller.NodeScaleTime);
		}

		private void ScaleNodeSize(GameObject node, float scale) {
			GetNodeRectTransform<Canvas>(node).localScale = Vector3.one * scale;
			GetNodeRectTransform<Image>(node).localScale = Vector3.one * scale;
		}
		
		private RectTransform GetNodeRectTransform<C>(GameObject node) where C: Component => node.GetComponentInChildren<C>().GetComponent<RectTransform>();

		private void AnimationEndAction(GameObject node, float scale) {
			if(scale == 0)
				controller.Nodes.Pool.Despawn(node);
			ActiveAnimations.Remove(node);
		}

		public float GetNodeTypeScale(NodeType type) => type == NodeType.ARTICLE ? 1 : CATEGORY_SCALE;
		
		private IEnumerator AnimateNodeSize(GameObject node, float scale, float time) {
			var canvasRect = GetNodeRectTransform<Canvas>(node);
			var imgRect = GetNodeRectTransform<Image>(node);
			float incAmount = (scale - canvasRect.localScale.x) * Time.deltaTime / time;
			while (true) {
				if (Mathf.Abs(canvasRect.localScale.x - scale) > Mathf.Abs(incAmount)) {
					var newScale = Vector3.one * (canvasRect.localScale.x + incAmount);
					canvasRect.localScale = newScale;
					imgRect.localScale = newScale;
					yield return null;
				} else {
					canvasRect.localScale = Vector3.one * scale;
					imgRect.localScale = Vector3.one * scale;
					break;
				}
			}
			AnimationEndAction(node, scale);
		}

		private void UpdateNodeObjectState(NodeType type, NodeState state, ref GameObject nodeObject) {
			nodeObject.GetComponent<SphereCollider>().enabled = state != NodeState.DISABLED;
			nodeObject.GetComponentInChildren<Image>().sprite = controller.NodeStateManager.GetStateSprite(type, state);
		}

	}
}