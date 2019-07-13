using System;
using System.Collections.Generic;
using Model;
using Services;
using UnityEngine;

namespace Controllers {
	public class ConnectionController: MonoBehaviour {
		public GraphObject Connections;

		private GraphController graphController;
		private NodeController nodeController;
		
		public List<GameObject> ActiveConnections { get; } = new List<GameObject>();

		void OnActiveNodeChanged(Node? node) {
			foreach (var connection in ActiveConnections) {
				Connections.Pool.Despawn(connection);
			}
			ActiveConnections.Clear();
			if(node == null) return;
			foreach (var child in node.Value.Children) {
				nodeController.LoadNode(child);
				GameObject connectionGO = Connections.Pool.Spawn();
				var childObj = GraphController.Graph.GetObjectFromId(child);
				InitializeConnection(ref connectionGO, GraphController.Graph.NodeObjectMap[node.Value], childObj);
			}
		}

		public void InitializeConnection(ref GameObject connectionObject, GameObject from, GameObject to) {
			connectionObject.transform.parent = Connections.Container.transform;
			var line = connectionObject.GetComponent<LineRenderer>();
			line.SetPositions(new [] {from.transform.position, to.transform.position});
			ActiveConnections.Add(connectionObject);
		}
		
		void Awake() {
			graphController = GetComponent<GraphController>();
			nodeController = GetComponent<NodeController>();
		}

		private void Start() {
			Connections.Pool = new GameObjectPool(Connections.Prefab, Connections.PreloadNumber, Connections.PoolContainer);
			graphController.OnActiveNodeChanged += OnActiveNodeChanged;
		}
	}
}