using System;
using Model;
using Services;
using Services.DataFiles;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Controllers {
	public class NodeController: MonoBehaviour {
		public NodeColors NodeColors;
		public NodeSprites NodeSprites;
		
		public GraphObject Nodes;
		
		public int NodeLoadedLimit;
		public bool LoadTestNodeSet;
		
		private NodeLoader nodeLoader;

		private GraphController graphController;

		public void LoadNode(uint id) {
			if(GraphController.Graph.IdNodeMap.ContainsKey(id)) return;
			Node node = nodeLoader.LoadNode(id);
			GraphController.Graph.IdNodeMap[id] = node;
			GameObject nodeGO = Nodes.Pool.Spawn();
			InitializeNode(node, ref nodeGO, Random.insideUnitSphere * graphController.WorldRadius);
			GraphController.Graph.NodeObjectMap[node] = nodeGO;
		}

		public void InitializeNode(Node model, ref GameObject nodeObject, Vector3 position) {
			nodeObject.transform.parent = Nodes.Container.transform;
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

		private void Start() {
			Nodes.Pool = new GameObjectPool(Nodes.Prefab, Nodes.PreloadNumber, Nodes.PoolContainer);
			nodeLoader = new NodeLoader(LoadTestNodeSet ? "-test" : "");
			for (uint i = 0; i < Math.Min(NodeLoadedLimit, nodeLoader.GetNodeNumber()); i++) {
				LoadNode(i);
			}
		}

		private void OnDestroy() {
			nodeLoader.Dispose();
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