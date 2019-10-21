using System.Collections.Generic;
using System.Linq;
using Controllers;
using Model;
using Model.Connection;
using UnityEngine;

namespace Services.Connection {
	public class ConnectionDistributionService {
		public readonly Node CentralNode;
		private readonly ConnectionController controller;
		private readonly ConnectionDistribution connectionDistribution;
		
		private List<Vector3> freePlaces = new List<Vector3>();
		private List<Vector3> takenPlaces = new List<Vector3>();

		public ConnectionDistributionService(Node centralNode, ConnectionController controller) {
			CentralNode = centralNode;
			this.controller = controller;
			connectionDistribution = controller.ConnectionDistribution;
			
			DistributeConnections();
		}

		private void DistributeConnections() {
			int connectionNumber = Mathf.Min(controller.GetNodeNeighbours(CentralNode).ToArray().Length, connectionDistribution.MaxVisibleConnections);
			int totalNumber = connectionNumber + connectionDistribution.ChangeConnectionNumber;
			int firstRowNumber = Mathf.Min(totalNumber, connectionDistribution.MaxRowConnections);
			
			DistributeAtElevation(firstRowNumber, connectionDistribution.RingAngleSpan.y);
			if(totalNumber > firstRowNumber)
				DistributeAtElevation(totalNumber - firstRowNumber, connectionDistribution.RingAngleSpan.x, 15);
		}

		private void DistributeAtElevation(int connectionNumber, float elevationAngle, float angleOffset = 0f) {
			Vector3 baseVector = Quaternion.AngleAxis(elevationAngle - 90, Vector3.forward) * Vector3.right * connectionDistribution.RingRadius;
			baseVector.y += 5;
			var rotation = Quaternion.AngleAxis(360f / connectionNumber, Vector3.up);
			if(angleOffset != 0)
				baseVector = Quaternion.AngleAxis(angleOffset, Vector3.up) * baseVector;
			for (int i = 0; i < connectionNumber; i++) {
				freePlaces.Add(baseVector);
				baseVector = rotation * baseVector;
			}
		}

		public void GenerateRoute(Model.Connection.Connection connection, Node to) {
			var basePos = NodePosition(CentralNode);
			var toPos = NodePosition(to);
			int nearestPlace = 0;
			float nearestDist = float.MaxValue;
			for (var i = 0; i < freePlaces.Count; i++) {
				float dist = Vector3.Distance(basePos + freePlaces[i], toPos);
				if (nearestDist > dist) {
					nearestDist = dist;
					nearestPlace = i;
				}
			}
			Vector3 chosenPlace = freePlaces[nearestPlace];
			freePlaces.RemoveAt(nearestPlace);
			takenPlaces.Add(chosenPlace);
			connection.Route = RouteService.GenerateRoute(NodePosition(CentralNode), NodePosition(to), chosenPlace);
		}

		public void OnConnectionUnloaded(Model.Connection.Connection connection) {
			takenPlaces.Remove(connection.Route.SpherePoint);
			freePlaces.Add(connection.Route.SpherePoint);
		}

		private Vector3 NodePosition(Node node) => GraphController.Graph.NodeObjectMap[node].transform.position;
	}
}