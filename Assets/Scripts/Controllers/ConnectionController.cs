using System;
using System.Collections.Generic;
using Model;
using Services;
using Services.ObjectPool;
using UnityEngine;

namespace Controllers {
	public class ConnectionController: MonoBehaviour {
		public GraphPooledObject Connections;

		private NodeController nodeController;
		
		public List<GameObject> ActiveConnections { get; } = new List<GameObject>();

		void OnActiveNodeChanged(Node node) {
			foreach (var connection in ActiveConnections) {
				Connections.Pool.Despawn(connection);
			}
			ActiveConnections.Clear();
			if(node == null) return;
			foreach (var child in node.Children) {
				GameObject connectionObject = Connections.Pool.Spawn();
				var childObj = GraphController.Graph.GetObjectFromId(child);
				InitializeConnection(ref connectionObject, GraphController.Graph.NodeObjectMap[node], childObj);
			}
		}

		public void InitializeConnection(ref GameObject connectionObject, GameObject from, GameObject to) {
			connectionObject.transform.parent = Connections.Container.transform;
			var line = connectionObject.GetComponent<LineRenderer>();
			line.SetPositions(new [] {from.transform.position, to.transform.position});
			ActiveConnections.Add(connectionObject);
		}
		
		void Awake() {
			nodeController = GetComponent<NodeController>();
		}

		private void Start() {
			Connections.Pool = new GameObjectPool(Connections.Prefab, Connections.PreloadNumber, Connections.PoolContainer);
			nodeController.OnSelectedNodeChanged += OnActiveNodeChanged;
		}
	}
}