using System;
using Model;
using UnityEngine;
using UnityEngine.UI;

namespace Controllers {
	public class NodeController: MonoBehaviour {
		public NodeColors NodeColors;
		public NodeSprites NodeSprites;

		public GraphController GraphController { get; private set; }

		public void InitializeNode(Node model, ref GameObject gameObject, Vector3 position) {
			gameObject.transform.parent = GraphController.Containers.NodeContainer.transform;
			gameObject.transform.position = position;
			gameObject.GetComponentInChildren<Text>().text = model.Title;
			var nodeImage = gameObject.GetComponentInChildren<Image>();
			nodeImage.sprite = model.Type == NodeType.ARTICLE ? NodeSprites.Article : NodeSprites.Category;
			nodeImage.color = NodeColors.Default;
			gameObject.name = model.ID.ToString();
		}

		public GameObject CreateNodeConnection(GameObject from, GameObject to) {
			GameObject connection = Instantiate(GraphController.ConnectionPrefab);
			connection.transform.parent = GraphController.Containers.ConnectionsContainer.transform;
			var line = connection.GetComponent<LineRenderer>();
			line.SetPositions(new [] {from.transform.position, to.transform.position});
			return connection;
		}

		public void UpdateNodeHighlight(GameObject newHighlight) {
			if(newHighlight == GraphController.LastHighlightedNode) return;
			if(GraphController.LastHighlightedNode != null)
				ChangeNodeHighlight(GraphController.LastHighlightedNode, false);
			GraphController.LastHighlightedNode = newHighlight;
			if(GraphController.LastHighlightedNode != null)
				ChangeNodeHighlight(GraphController.LastHighlightedNode, true);
		}

		private void ChangeNodeHighlight(GameObject node, bool highlight) {
			node.GetComponentInChildren<Text>().enabled = highlight;
			node.GetComponentInChildren<Image>().color = highlight ? NodeColors.Highlighted : NodeColors.Default;
		}
		
		void Awake() {
			GraphController = GetComponent<GraphController>();
		}

		private void Start() {
			GraphController.OnActiveNodeChanged += OnActiveNodeChanged;
		}

		void OnActiveNodeChanged(Node? node) {
			foreach (Transform child in GraphController.Containers.ConnectionsContainer.transform) {
				Destroy(child.gameObject);
			}
			if(node == null) return;
			foreach (var child in node.Value.Children) {
				var childObj = GraphController.GetObjectFromId(child);
				if(childObj != null)
					CreateNodeConnection(GraphController.NodeObjectMap[node.Value], childObj);
			}
		}

		void Update() {
		}
	}
	
	[Serializable]
	public class NodeColors {
		public Color Default;
		public Color Highlighted;
	}
	
	[Serializable]
	public class NodeSprites {
		public Sprite Article;
		public Sprite Category;
	}
}