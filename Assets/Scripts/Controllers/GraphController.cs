using System;
using Model;
using Services;
using UnityEngine;

namespace Controllers {
	public class GraphController : MonoBehaviour {
		public GameObject Infographic; //TODO: move
		
		public float WorldRadius;
		
		public static Graph Graph { get; } = new Graph();
		
		private NodeController nodeController;
		
		private GraphMode graphMode = GraphMode.FREE_FLIGHT;
		public GraphMode GraphMode {
			get { return graphMode; }
			set {
				graphMode = value;
				if (graphMode == GraphMode.FREE_FLIGHT)
					nodeController.SelectedNode = null;
			}
		}
		
		void Awake() {
			nodeController = GetComponent<NodeController>();
		}
	}

	[Serializable]
	public class GraphObject {
		public GameObject Container;
		public GameObject Prefab;
		public GameObjectPool Pool;
		public int PreloadNumber;

		private static readonly string POOL_OBJECT_NAME = "Pool";
		public GameObject PoolContainer => Container.transform.Find(POOL_OBJECT_NAME).gameObject;
	}

	public enum GraphMode {
		FREE_FLIGHT, NODE_TRAVERSE
	}
}
