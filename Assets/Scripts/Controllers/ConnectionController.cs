using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Model;
using Services;
using Services.Connection;
using Services.ObjectPool;
using UnityEngine;

namespace Controllers {
	public class ConnectionController: MonoBehaviour {
		public GraphPooledObject Connections;
		public int MaxVisibleConnections;
		public float ScrollInterval;
		
		public Color ChildConnectionColor;
		public Color ParentConnectionColor;
		
		private ConnectionService connectionService;
		
		private List<GameObject> ActiveConnections { get; } = new List<GameObject>();
		private int currentVisibleIndex = 0;
		private int scrollDirection;
		private float scrollTimer;

		public List<Node> GetActiveNodeConnections() {
			if (nodeController.SelectedNode == null) return null;
			var connections = nodeController.SelectedNode.GetConnections(graphController.ConnectionMode.Value).Select(id => {
				nodeController.LoadNode(id); //TODO handle loading in separate controller
				return GraphController.Graph.IdNodeMap[id];
			}).ToList();
			return Utils.GetCurcularListPart(connections, currentVisibleIndex, MaxVisibleConnections);
		}

		public void OnScrollInputChanged(int direction) {
			scrollDirection = direction;
		}

		private void UpdateConnections() {
			foreach (var connection in ActiveConnections) {
				Connections.Pool.Despawn(connection);
			}
			ActiveConnections.Clear();
			if(nodeController.SelectedNode == null) return;
			foreach (var connection in GetActiveNodeConnections()) {
				GameObject connectionObject = Connections.Pool.Spawn();
				var childObj = GraphController.Graph.NodeObjectMap[connection];
				InitializeConnection(ref connectionObject, GraphController.Graph.NodeObjectMap[nodeController.SelectedNode], childObj);
			}
		}

		private void InitializeConnection(ref GameObject connectionObject, GameObject from, GameObject to) {
			var basePosition = from.transform.position;
			connectionObject.name = to.name;
			connectionObject.transform.position = basePosition;
			connectionObject.transform.parent = Connections.Container.transform;
			
			var line = connectionObject.GetComponent<LineRenderer>();
			line.material.color = graphController.ConnectionMode.Value == ConnectionMode.PARENTS ? ParentConnectionColor : ChildConnectionColor;
			
			Connection connectionModel = ConnectionService.GenerateConnection(basePosition, to.transform.position);
			line.positionCount = connectionModel.SegmentPoints.Length;
			line.SetPositions(connectionModel.SegmentPoints);
			ActiveConnections.Add(connectionObject);
		}
		
		private void resetTimer() {
			scrollTimer = ScrollInterval * 1000;
		}

		#region Mono Behaviour

		private NodeController nodeController;
		private GraphController graphController;
		
		void Awake() {
			graphController = GetComponent<GraphController>();
			nodeController = GetComponent<NodeController>();
		}

		private void Start() {
			Connections.Pool = new GameObjectPool(Connections.Prefab, Connections.PreloadNumber, Connections.PoolContainer);
			nodeController.OnSelectedNodeChanged += mode => UpdateConnections();
			graphController.ConnectionMode.OnValueChanged += mode => UpdateConnections();
			
			connectionService = new ConnectionService(ref graphController.ConnectionMode.OnValueChanged);
			resetTimer();
		}

		private void Update() {
			if (scrollDirection != 0) {
				scrollTimer -= Time.deltaTime * 1000;
				if (scrollTimer <= 0) {
					currentVisibleIndex += scrollDirection;
					UpdateConnections();
					resetTimer();
				}
			}
		}

		#endregion
	}
}