using System;
using UnityEngine;

namespace Controllers.UI {
	public class GridController : MonoBehaviour {

		public GameObject Entity;
		public GameObject Graph;
		public GridConfig Config;

		private NetworkController networkController;

		void Awake() {
			networkController = Graph.GetComponent<NetworkController>();
		}

		// Use this for initialization
		void Start() {
			transform.GetComponent<MeshRenderer>().sortingOrder = 50;
		}

		// Update is called once per frame
		void Update() {
			if (networkController.IsClient()) {
				return;
			}
			transform.position = new Vector3(Entity.transform.position.x, Entity.transform.position.y + Config.GridHeight, Entity.transform.position.z);
		}

		[Serializable]
		public class GridConfig {
			public float GridHeight = -6f;
		}

	}
}