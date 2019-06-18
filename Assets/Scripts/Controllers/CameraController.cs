using System;
using UnityEngine;

namespace Controllers {
	public class CameraController: MonoBehaviour {
		public GameObject Entity;
		public NodeController NodeController { get; private set; }

		private Vector3 velocity = Vector3.zero;
		public float smoothTime = 0.9F;
		
		void Start() {
			NodeController = GetComponent<NodeController>();
		}

		void Update() {
			if(NodeController.ActiveNode == null) return;
			Vector3 targetPosition = NodeController.ActiveNode.transform.TransformPoint(new Vector3(0, 3, 0));
			Entity.transform.position = Vector3.SmoothDamp(Entity.transform.position, targetPosition, ref velocity, smoothTime);
		}
	}
}