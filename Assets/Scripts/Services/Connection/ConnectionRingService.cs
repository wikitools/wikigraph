using Controllers;
using Model;
using Model.Connection;
using UnityEngine;

namespace Services.Connection {
	public class ConnectionRingService {
		private readonly ConnectionRing connectionRing;
		private readonly RouteService routeService = new RouteService();

		public ConnectionRingService(ConnectionRing connectionRing) {
			this.connectionRing = connectionRing;
		}

		public Route GenerateRoute(Model.Connection.Connection connection, Node centralNode) {
			Node satelliteNode = connection.Item1 == centralNode ? connection.Item2 : connection.Item1;
			return routeService.GenerateRoute(NodePosition(centralNode), NodePosition(satelliteNode));
		}

		private Vector3 NodePosition(Node node) => GraphController.Graph.NodeObjectMap[node].transform.position;
	}
}