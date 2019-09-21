using System;
using System.Linq;
using System.Collections.Generic;
using Controllers;
using Model;
using UnityEngine;

namespace Services.Connection {
	public class ConnectionManager {
		private readonly Logger<ConnectionManager> logger = new Logger<ConnectionManager>();
		
		private readonly ConnectionController controller;

		private readonly RouteService routeService = new RouteService();
		private Graph graph => GraphController.Graph;

		public ConnectionManager(ConnectionController controller) {
			this.controller = controller;
		}

		#region Connection Loading

		public void LoadConnection(Model.Connection connection) {
			if (graph.ConnectionObjectMap.ContainsKey(connection))
				return;
			InitConnection(connection);
			controller.OnConnectionLoaded?.Invoke(connection);
		}

		public void UnloadConnection(Model.Connection connection) {
			if (!graph.ConnectionObjectMap.ContainsKey(connection))
				return;
			controller.Connections.Pool.Despawn(graph.ConnectionObjectMap[connection]);
			graph.ConnectionObjectMap.Remove(connection);
			controller.OnConnectionUnloaded?.Invoke(connection);
		}

		#endregion

		#region Connection Creation

		private bool CanLoadConnection(Model.Connection connection) =>
			graph.IdNodeMap.ContainsKey(connection.Item1.ID) && graph.IdNodeMap.ContainsKey(connection.Item2.ID);

		private void InitConnection(Model.Connection connection) {
			if (!connection.Item1.GetConnections(controller.GraphController.ConnectionMode.Value).Contains(connection.Item2.ID) && controller.NetworkController.IsServer())
				logger.Warning("Attempting to create connection that does not exist");

			GameObject connectionObject = controller.Connections.Pool.Spawn();
			connection.Route = InitConnectionObject(ref connectionObject, graph.NodeObjectMap[connection.Item1], 
				graph.NodeObjectMap[connection.Item2], GetConnectionLineColor(connection));
			graph.ConnectionObjectMap.Add(connection, connectionObject);
		}

		private Route InitConnectionObject(ref GameObject connectionObject, GameObject from, GameObject to, Color color) {
			var basePosition = from.transform.position;
			connectionObject.name = from.name + " " + to.name;
			connectionObject.transform.position = basePosition;
			connectionObject.transform.parent = controller.Connections.Container.transform;

			var line = connectionObject.GetComponent<LineRenderer>();
			line.material.color = color;

			Route route = routeService.GenerateRoute(basePosition, to.transform.position);
			line.positionCount = route.SegmentPoints.Length;
			line.SetPositions(route.SegmentPoints);
			return route;
		}

		public void SetConnectionLineColor(Model.Connection connection) {
			graph.ConnectionObjectMap[connection].GetComponent<LineRenderer>().material.color = GetConnectionLineColor(connection);
		}

		private Color GetConnectionLineColor(Model.Connection connection) {
			if (!connection.Ends.Contains(controller.NodeController.SelectedNode))
				return controller.Colors.DisabledColor;
			return controller.GraphController.ConnectionMode.Value == ConnectionMode.PARENTS ? controller.Colors.ParentColor : controller.Colors.ChildColor;
		}

		#endregion
	}
}