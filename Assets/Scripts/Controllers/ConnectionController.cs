using System;
using System.Collections.Generic;
using System.Linq;
using Model;
using Services;
using Services.ObjectPool;
using UnityEngine;

namespace Controllers {
	public class ConnectionController: MonoBehaviour {
		public GraphPooledObject Connections;
		
		public List<GameObject> ActiveConnections { get; } = new List<GameObject>();

		public Node[] GetActiveNodeConnections() {
			if (nodeController.SelectedNode == null) return null;
			return nodeController.SelectedNode.GetConnections(graphController.ConnectionMode.Value).Select(id => {
				nodeController.LoadNode(id); //TODO handle loading in separate controller
				return GraphController.Graph.IdNodeMap[id];
			}).ToArray();
		}

		private void OnActiveNodeChanged(Node node) {
			foreach (var connection in ActiveConnections) {
				Connections.Pool.Despawn(connection);
			}
			ActiveConnections.Clear();
			if(node == null) return;
			foreach (var connection in GetActiveNodeConnections()) {
				GameObject connectionObject = Connections.Pool.Spawn();
				var childObj = GraphController.Graph.NodeObjectMap[connection];
				InitializeConnection(ref connectionObject, GraphController.Graph.NodeObjectMap[node], childObj);
			}
		}

		public void InitializeConnection(ref GameObject connectionObject, GameObject from, GameObject to) {
			connectionObject.transform.parent = Connections.Container.transform;
			var line = connectionObject.GetComponent<LineRenderer>();
			line.SetPositions(new [] {from.transform.position, to.transform.position});
			ActiveConnections.Add(connectionObject);
		}

		#region MonoBehaviour

		private NodeController nodeController;
		private GraphController graphController;
		
		void Awake() {
			graphController = GetComponent<GraphController>();
			nodeController = GetComponent<NodeController>();
		}

		private void Start() {
			Connections.Pool = new GameObjectPool(Connections.Prefab, Connections.PreloadNumber, Connections.PoolContainer);
			nodeController.OnSelectedNodeChanged += OnActiveNodeChanged;
			graphController.ConnectionMode.OnValueChanged += mode => OnActiveNodeChanged(nodeController.SelectedNode);
		}

		#endregion
	}
}