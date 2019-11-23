using System;
using System.Collections.Generic;
using System.Linq;
using Controllers;
using Model;
using Model.Connection;
using UnityEngine;

namespace Services.Connection {
	public class ConnectionDistributionService {
		private readonly Logger<ConnectionDistributionService> logger = new Logger<ConnectionDistributionService>();
		
		public readonly Node CentralNode;
		private readonly ConnectionController controller;
		private readonly ConnectionDistribution distribution;
		
		private List<Vector3> freePlaces = new List<Vector3>();
		private List<Vector3> takenPlaces = new List<Vector3>();

		public ConnectionDistributionService(Node centralNode, ConnectionController controller) {
			CentralNode = centralNode;
			this.controller = controller;
			distribution = controller.ConnectionDistribution;
			if (distribution.ChangeBy > distribution.MaxVisibleNumber) {
				logger.Warning("Connections can't change by amount grater than the max visible number is");
				distribution.ChangeBy = distribution.MaxVisibleNumber;
			}
			DistributeConnections();
		}

		private void DistributeConnections() {
			int connectionNumber = Mathf.Min(controller.GetNodeNeighbours(CentralNode).ToArray().Length, distribution.MaxVisibleNumber);
			int totalNumber = connectionNumber + distribution.ChangeBy + 2;
			int firstRowNumber = Mathf.Min(totalNumber, Mathf.RoundToInt(distribution.MaxVisibleNumber * .75f));
			
			DistributeAtElevation(firstRowNumber, distribution.RingAngleSpan.y);
			if(totalNumber > firstRowNumber)
				DistributeAtElevation(totalNumber - firstRowNumber, distribution.RingAngleSpan.x, 15);
		}

		private void DistributeAtElevation(int connectionNumber, float elevationAngle, float angleOffset = 0f) {
			Vector3 baseVector = Quaternion.AngleAxis(elevationAngle - 90, Vector3.forward) * Vector3.right * distribution.RingRadius;
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
			if (freePlaces.Count == 0) 
				DistributeAtElevation(distribution.ChangeBy, (distribution.RingAngleSpan.x + distribution.RingAngleSpan.y) / 2, 15);
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
			connection.ConnectionRoute = ConnectionRouteService.GenerateConnectionRoute(NodePosition(CentralNode), NodePosition(to), chosenPlace);
		}

		public void OnConnectionUnloaded(Model.Connection.Connection connection) {
			takenPlaces.Remove(connection.ConnectionRoute.SpherePoint);
			freePlaces.Add(connection.ConnectionRoute.SpherePoint);
		}

		private Vector3 NodePosition(Node node) => GraphController.Graph.NodeObjectMap[node].transform.position;
	}
	
	[Serializable]
	public class ConnectionDistribution {
		public int MaxVisibleNumber = 8;
		public int ChangeBy = 2;
		
		public Vector2 RingAngleSpan = new Vector2(50, 80);
		public float RingRadius = 5;
	}
}