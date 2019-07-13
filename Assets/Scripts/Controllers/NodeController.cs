using System;
using Model;
using UnityEngine;
using UnityEngine.UI;

namespace Controllers {
	public class NodeController: MonoBehaviour {
		public NodeColors NodeColors;
		public NodeSprites NodeSprites;

		private GraphController graphController;

		public void InitializeNode(Node model, ref GameObject nodeObject, Vector3 position) {
			nodeObject.transform.parent = graphController.Nodes.Container.transform;
			nodeObject.transform.position = position;
			nodeObject.GetComponentInChildren<Text>().text = model.Title;
			var nodeImage = nodeObject.GetComponentInChildren<Image>();
			nodeImage.sprite = model.Type == NodeType.ARTICLE ? NodeSprites.Article : NodeSprites.Category;
			nodeImage.color = NodeColors.Default;
			nodeObject.name = model.ID.ToString();
		}

		public void UpdateNodeHighlight(GameObject newHighlight) {
			if(newHighlight == graphController.LastHighlightedNode) return;
			if(graphController.LastHighlightedNode != null)
				ChangeNodeHighlight(graphController.LastHighlightedNode, false);
			graphController.LastHighlightedNode = newHighlight;
			if(graphController.LastHighlightedNode != null)
				ChangeNodeHighlight(graphController.LastHighlightedNode, true);
		}

		private void ChangeNodeHighlight(GameObject node, bool highlight) {
			node.GetComponentInChildren<Text>().enabled = highlight;
			node.GetComponentInChildren<Image>().color = highlight ? NodeColors.Highlighted : NodeColors.Default;
		}
		
		void Awake() {
			graphController = GetComponent<GraphController>();
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