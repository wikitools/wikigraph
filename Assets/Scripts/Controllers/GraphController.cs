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

	public enum GraphMode {
		FREE_FLIGHT, NODE_TRAVERSE
	}
}
