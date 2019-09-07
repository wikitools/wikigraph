using UnityEngine;

namespace Controllers {
	public class CameraController : MonoBehaviour {
		public GameObject Entity;
		public NodeController NodeController { get; private set; }
		public GraphController GraphController { get; private set; }



		private Vector3 velocity = Vector3.zero;
		public float smoothTime = 0.9F;

		void Awake() {
			NodeController = GetComponent<NodeController>();
			GraphController = GetComponent<GraphController>();
		}

		void Update() {
			if (GraphController.GraphMode.Value == GraphMode.NODE_TRAVERSE) {
				if (NodeController.SelectedNode != null) {
					Vector3 targetPosition = GraphController.Graph.NodeObjectMap[NodeController.SelectedNode].transform.TransformPoint(new Vector3(0, 3, 0));
					Entity.transform.position = Vector3.SmoothDamp(Entity.transform.position, targetPosition, ref velocity, smoothTime);
				}
			}
		}
	}
}