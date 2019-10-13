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
		private List<Vector3> usedPlaces = new List<Vector3>();
		
		private List<GameObject> debugMarks = new List<GameObject>();

		public ConnectionDistributionService(Node centralNode, ConnectionController controller) {
			CentralNode = centralNode;
			this.controller = controller;
			connectionDistribution = controller.ConnectionDistribution;
			
			DistributeConnections();
			
			//Debug
			var pos = GraphController.Graph.NodeObjectMap[centralNode].transform.position;
			freePlaces.ForEach(place => debugMarks.Add(Object.Instantiate(controller.ConnectionMarker, pos + place, Quaternion.identity, controller.transform)));
		}

		private void DistributeConnections() {
			int connectionNumber = Mathf.Min(controller.GetNodeNeighbours(CentralNode).ToArray().Length, connectionDistribution.MaxVisibleConnections);
			int totalNumber = connectionNumber + connectionDistribution.ChangeConnectionNumber;
			int firstRowNumber = Mathf.Min(totalNumber, connectionDistribution.MaxRowConnections);
			
			DistributeAtElevation(firstRowNumber, connectionDistribution.RingAngleSpan.y);
			if(totalNumber > firstRowNumber)
				DistributeAtElevation(totalNumber - firstRowNumber, connectionDistribution.RingAngleSpan.x);
		}

		private void DistributeAtElevation(int connectionNumber, float elevationAngle) {
			Vector3 baseVector = Quaternion.AngleAxis(elevationAngle - 90, Vector3.forward) * Vector3.right * connectionDistribution.RingRadius;
			baseVector.y += 5;
			float angleDistance = 360f / connectionNumber;
			for (int i = 0; i < connectionNumber; i++) {
				freePlaces.Add(baseVector);
				baseVector = Quaternion.AngleAxis(angleDistance, Vector3.up) * baseVector;
			}
		}

		public Route GenerateRoute(Node to) {
			return RouteService.GenerateRoute(NodePosition(CentralNode), NodePosition(to));
		}

		private Vector3 NodePosition(Node node) => GraphController.Graph.NodeObjectMap[node].transform.position;
	}
}