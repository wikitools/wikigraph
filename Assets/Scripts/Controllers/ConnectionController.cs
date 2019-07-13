using System.Collections.Generic;
using UnityEngine;

namespace Controllers {
	public class ConnectionController: MonoBehaviour {
		public GraphController GraphController { get; private set; }
		
		public List<GameObject> ActiveConnections = new List<GameObject>();

		public void InitializeConnection(ref GameObject connectionObject, GameObject from, GameObject to) {
			connectionObject.transform.parent = GraphController.Connections.Container.transform;
			var line = connectionObject.GetComponent<LineRenderer>();
			line.SetPositions(new [] {from.transform.position, to.transform.position});
			ActiveConnections.Add(connectionObject);
		}
		
		void Awake() {
			GraphController = GetComponent<GraphController>();
		}
	}
}