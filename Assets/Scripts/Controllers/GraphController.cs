using System;
using Model;
using Services;
using UnityEngine;

namespace Controllers {
	public class GraphController : MonoBehaviour {
		public GameObject Infographic; //TODO: move
		
		public float WorldRadius;
		
		public static Graph Graph { get; } = new Graph();
		
		private GraphMode graphMode = GraphMode.FREE_FLIGHT;
		public GraphMode GraphMode {
			get { return graphMode; }
			set {
				graphMode = value;
				if (graphMode == GraphMode.FREE_FLIGHT)
					ActiveNode = null;
			}
		}

		public GameObject LastHighlightedNode { get; set; }
		
		private GameObject activeNode;
		public GameObject ActiveNode {
			get { return activeNode; }
			set {
				if(activeNode == value) return;
				activeNode = value;
				GraphMode = activeNode != null ? GraphMode.NODE_TRAVERSE : GraphMode.FREE_FLIGHT;
				OnActiveNodeChanged?.Invoke(Graph.GetNodeFromObject(activeNode));
			}
		}

		public Action<Node?> OnActiveNodeChanged;
	}

	[Serializable]
	public class GraphObject {
		public GameObject Container;
		public GameObject Prefab;
		public GameObjectPool Pool;
		public int PreloadNumber;

		public static readonly string POOL_GO_NAME = "Pool";
		public GameObject PoolContainer => Container.transform.Find(POOL_GO_NAME).gameObject;
	}

	public enum GraphMode {
		FREE_FLIGHT, NODE_TRAVERSE
	}
}
