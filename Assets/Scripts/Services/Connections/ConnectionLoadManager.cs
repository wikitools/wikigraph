using System.Linq;
using Controllers;
using Model;
using Model.Connection;
using UnityEngine;

namespace Services.Connection {
	public class ConnectionLoadManager {
		private readonly Logger<ConnectionLoadManager> logger = new Logger<ConnectionLoadManager>();
		
		private readonly ConnectionController controller;

		private Graph graph => GraphController.Graph;

		public ConnectionLoadManager(ConnectionController controller) {
			this.controller = controller;
		}

		#region Connection Loading

		public void LoadConnection(Model.Connection.Connection connection, ConnectionDistributionService distributionService) {
			if (graph.ConnectionObjectMap.ContainsKey(connection))
				return;
			InitConnection(connection, distributionService);
			controller.OnConnectionLoaded?.Invoke(connection);
		}

		public void UnloadConnection(Model.Connection.Connection connection) {
			if (!graph.ConnectionObjectMap.ContainsKey(connection))
				return;
			controller.Connections.Pool.Despawn(graph.ConnectionObjectMap[connection]);
			graph.ConnectionObjectMap.Remove(connection);

			if (graph.ConnectionNodes.ContainsKey(connection))
				controller.NodeController.NodeLoadManager.UnloadConnectionNode(connection);
			controller.OnConnectionUnloaded?.Invoke(connection);
		}

		#endregion

		#region Connection Creation

		private void InitConnection(Model.Connection.Connection connection, ConnectionDistributionService distributionService) {
			if (!(Connected(connection.Item1, connection.Item2) || Connected(connection.Item2, connection.Item1)) && controller.NetworkController.IsServer())
				logger.Warning("Attempting to create connection that does not exist");

			GameObject connectionObject = controller.Connections.Pool.Spawn();
			Node otherNode = connection.OtherEnd(distributionService.CentralNode);
			distributionService.GenerateRoute(connection, otherNode);
			var centerNode = graph.NodeObjectMap[distributionService.CentralNode];
			InitConnectionObject(ref connectionObject, connection.Route, centerNode, 
				graph.NodeObjectMap[otherNode], GetConnectionLineColor(connection));
			graph.ConnectionObjectMap.Add(connection, connectionObject);

			if (distributionService.CentralNode == controller.NodeController.SelectedNode) {
				GameObject conNode = controller.NodeController.NodeLoadManager.LoadConnectionNode(otherNode, centerNode.transform.position + connection.Route.SpherePoint);
				graph.ConnectionNodes.Add(connection, conNode);
			}
		}

		private bool Connected(Node one, Node two) => one.GetConnections(controller.GraphController.ConnectionMode.Value).Contains(two.ID);

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