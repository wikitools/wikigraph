using System;
using System.Linq;
using System.Collections.Generic;
using Controllers;
using Model;
using UnityEngine;

namespace Services.Connection {
	public class ConnectionManager {
		private readonly Logger<ConnectionManager> logger = new Logger<ConnectionManager>();
		private readonly List<Tuple<uint, uint>> connectionQueue = new List<Tuple<uint, uint>>();
		private readonly ConnectionController controller;
		private readonly ObservableProperty<ConnectionMode> connectionMode;
		private readonly Func<bool> isServer;

		private readonly RouteService routeService = new RouteService();
		private Graph graph => GraphController.Graph;

		public ConnectionManager(ConnectionController controller, ObservableProperty<ConnectionMode> connectionMode, Func<bool> isServer) {
			this.controller = controller;
			this.connectionMode = connectionMode;
			this.isServer = isServer;
		}

		#region Connection Loading

		public void LoadConnection(Tuple<uint, uint> nodeIDs) {
			if (!CanLoadConnection(nodeIDs)) {
				connectionQueue.Add(nodeIDs);
				return;
			}
			DoLoadConnection(nodeIDs);
		}

		private void DoLoadConnection(Tuple<uint, uint> nodeIDs) {
			var connection = CreateConnection(nodeIDs);

			if (graph.ConnectionObjectMap.ContainsKey(connection))
				return;
			InitConnection(connection);
			controller.OnConnectionLoaded?.Invoke(connection);
		}

		public void UnloadConnection(Tuple<uint, uint> nodeIDs) {
			var connection = CreateConnection(nodeIDs);

			if (!graph.ConnectionObjectMap.ContainsKey(connection))
				return;
			controller.Connections.Pool.Despawn(graph.ConnectionObjectMap[connection]);
			graph.ConnectionObjectMap.Remove(connection);
			controller.OnConnectionUnloaded?.Invoke(connection);
		}

		public void CheckConnectionQueue() {
			for (var i = connectionQueue.Count - 1; i >= 0; i--) {
				if (CanLoadConnection(connectionQueue[i])) {
					DoLoadConnection(connectionQueue[i]);
					connectionQueue.RemoveAt(i);
				}
			}
		}

		#endregion

		#region Connection Creation

		private Model.Connection CreateConnection(Tuple<uint, uint> connection) {
			return new Model.Connection(graph.IdNodeMap[connection.Item1], graph.IdNodeMap[connection.Item2]);
		}

		private bool CanLoadConnection(Tuple<uint, uint> connection) =>
			graph.IdNodeMap.ContainsKey(connection.Item1) && graph.IdNodeMap.ContainsKey(connection.Item2);

		private void InitConnection(Model.Connection connection) {
			if (!connection.Item1.GetConnections(connectionMode.Value).Contains(connection.Item2.ID) && isServer())
				logger.Warning("Attempting to create connection that does not exist");

			GameObject connectionObject = controller.Connections.Pool.Spawn();
			connection.Route = InitConnectionObject(ref connectionObject, graph.NodeObjectMap[connection.Item1], 
				graph.NodeObjectMap[connection.Item2], GetConnectionLineColor(connection));
			graph.ConnectionObjectMap.Add(connection, connectionObject);
		}

		private Route InitConnectionObject(ref GameObject connectionObject, GameObject from, GameObject to, Color color) {
			var basePosition = from.transform.position;
			connectionObject.name = to.name;
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
			return connectionMode.Value == ConnectionMode.PARENTS ? controller.Colors.ParentColor : controller.Colors.ChildColor;
		}

		#endregion
	}
}