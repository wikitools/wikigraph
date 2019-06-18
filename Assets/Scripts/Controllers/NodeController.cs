using System;
using Model;
using UnityEngine;
using UnityEngine.UI;

namespace Controllers {
	public class NodeController: MonoBehaviour {
		public NodeColors NodeColors;
		public NodeSprites NodeSprites;
		
		private GameObject lastHighlightedNode;
		public GameObject ActiveNode { get; set; }

		public void InitializeNode(Node model, ref GameObject gameObject, Vector3 position) {
			gameObject.transform.parent = transform;
			gameObject.transform.position = position;
			gameObject.GetComponentInChildren<Text>().text = model.Title;
			var nodeImage = gameObject.GetComponentInChildren<Image>();
			nodeImage.sprite = model.Type == NodeType.ARTICLE ? NodeSprites.Article : NodeSprites.Category;
			nodeImage.color = NodeColors.Default;
		}

		public void UpdateNodeHighlight(GameObject newHighlight) {
			if(newHighlight == lastHighlightedNode) return;
			if(lastHighlightedNode != null)
				ChangeNodeHighlight(lastHighlightedNode, false);
			lastHighlightedNode = newHighlight;
			if(lastHighlightedNode != null)
				ChangeNodeHighlight(lastHighlightedNode, true);
		}

		private void ChangeNodeHighlight(GameObject node, bool highlight) {
			node.GetComponentInChildren<Text>().enabled = highlight;
			node.GetComponentInChildren<Image>().color = highlight ? NodeColors.Highlighted : NodeColors.Default;
		}
		
		void Start() {
			
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