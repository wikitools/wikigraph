using System.Linq;
using Controllers;
using Model;
using Model.Connection;
using UnityEngine;

namespace Services.Connection {
	public class ConnectionManager {
		private readonly Logger<ConnectionManager> logger = new Logger<ConnectionManager>();
		
		private readonly ConnectionController controller;

		private Graph graph => GraphController.Graph;
		private ConnectionRingService connectionRingService;

		public ConnectionManager(ConnectionController controller) {
			this.controller = controller;
			connectionRingService = new ConnectionRingService(controller.ConnectionRing);
		}

		#region Connection Loading

		public void LoadConnection(Model.Connection.Connection connection, Node centralNode) {
			if (graph.ConnectionObjectMap.ContainsKey(connection))
				return;
			InitConnection(connection, centralNode);
			controller.OnConnectionLoaded?.Invoke(connection);
		}

		public void UnloadConnection(Model.Connection.Connection connection) {
			if (!graph.ConnectionObjectMap.ContainsKey(connection))
				return;
			controller.Connections.Pool.Despawn(graph.ConnectionObjectMap[connection]);
			graph.ConnectionObjectMap.Remove(connection);
			controller.OnConnectionUnloaded?.Invoke(connection);
		}

		#endregion

		#region Connection Creation

		private void InitConnection(Model.Connection.Connection connection, Node centralNode) {
			if (!connection.Item1.GetConnections(controller.GraphController.ConnectionMode.Value).Contains(connection.Item2.ID) && controller.NetworkController.IsServer())
				logger.Warning("Attempting to create connection that does not exist");

			GameObject connectionObject = controller.Connections.Pool.Spawn();
			connection.Route = connectionRingService.GenerateRoute(connection, centralNode);
			InitConnectionObject(ref connectionObject, connection.Route, graph.NodeObjectMap[connection.Item1], 
				graph.NodeObjectMap[connection.Item2], GetConnectionLineColor(connection));
			graph.ConnectionObjectMap.Add(connection, connectionObject);
		}

		private void InitConnectionObject(ref GameObject connectionObject, Route route, GameObject from, GameObject to, Color color) {
			var basePosition = from.transform.position;
			connectionObject.name = from.name + " " + to.name;
			connectionObject.transform.position = basePosition;
			connectionObject.transform.parent = controller.Connections.Container.transform;

			var line = connectionObject.GetComponent<LineRenderer>();
			line.material.color = color;

			line.positionCount = route.SegmentPoints.Length;
			line.SetPositions(route.SegmentPoints);
		}

		public void SetConnectionLineColor(Model.Connection.Connection connection) {
			graph.ConnectionObjectMap[connection].GetComponent<LineRenderer>().material.color = GetConnectionLineColor(connection);
		}

		private Color GetConnectionLineColor(Model.Connection.Connection connection) {
			if (!connection.Ends.Contains(controller.NodeController.SelectedNode))
				return controller.Colors.DisabledColor;
			return controller.GraphController.ConnectionMode.Value == ConnectionMode.PARENTS ? controller.Colors.ParentColor : controller.Colors.ChildColor;
		}

		#endregion
	}
}