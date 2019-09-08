using UnityEngine;
using UnityEngine.Serialization;

namespace Controllers {
	public class CameraController : MonoBehaviour {
		public GameObject Entity;
		private NodeController nodeController;
		private GraphController graphController;
		private NetworkController networkController;
		private Vector3 velocity = Vector3.zero;
		public float SmoothTime = 0.9F;

		void Awake() {
			nodeController = GetComponent<NodeController>();
			graphController = GetComponent<GraphController>();
			networkController = GetComponent<NetworkController>();
		}

		void Update() {
			if (!networkController.IsServer())
				return;
			if (graphController.GraphMode.Value == GraphMode.NODE_TRAVERSE) {
				if (nodeController.SelectedNode != null) {
					Vector3 targetPosition = GraphController.Graph.NodeObjectMap[nodeController.SelectedNode].transform.TransformPoint(new Vector3(0, 3, 0));
					Entity.transform.position = Vector3.SmoothDamp(Entity.transform.position, targetPosition, ref velocity, SmoothTime);
				}
			}
		}
	}
}