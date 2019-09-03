using System;
using System.Collections.Generic;
using System.Linq;
using Model;
using Services;
using Services.Connection;
using Services.ObjectPool;
using UnityEngine;

namespace Controllers {
	public class ConnectionController: MonoBehaviour {
		public GraphPooledObject Connections;
		public int MaxVisibleConnections;
		public int ChangeConnectionNumber;
		public float ScrollInterval;
		
		public Color ChildConnectionColor;
		public Color ParentConnectionColor;

		public Action<Node> OnConnectionCreated;
		public Action<Node> OnConnectionRemoved;
		
		private ConnectionService connectionService;
		
		private List<Node> ActiveConnections { get; } = new List<Node>();
		private Dictionary<Node, GameObject> ConnectionObjectMap = new Dictionary<Node, GameObject>();
		
		private int currentVisibleIndex;
		private int scrollDirection;
		private float scrollTimer;

		public void OnScrollInputChanged(int direction) {
			scrollDirection = direction;
		}

		private void UpdateVisibleConnections() {
			if (ActiveConnections.Count <= MaxVisibleConnections) {
				ConnectionObjectMap.Values.ToList().ForEach(connection => connection.SetActive(true));
				return;
			}
			Utils.GetCircularListPart(ActiveConnections, currentVisibleIndex, MaxVisibleConnections).ForEach(UnloadConnection);
			currentVisibleIndex = Utils.Mod(currentVisibleIndex + scrollDirection * ChangeConnectionNumber, ActiveConnections.Count);
			Utils.GetCircularListPart(ActiveConnections, currentVisibleIndex, MaxVisibleConnections).ForEach(LoadConnection);
		}

		public void LoadConnection(Node node) {
			ConnectionObjectMap[node].SetActive(true);
			OnConnectionCreated?.Invoke(node);
		}

		public void UnloadConnection(Node node) {
			ConnectionObjectMap[node].SetActive(false);
			OnConnectionRemoved?.Invoke(node);
		}

		private void LoadConnections() {
			ConnectionObjectMap.Values.ToList().ForEach(connection => Connections.Pool.Despawn(connection));
			ConnectionObjectMap.Clear();
			ActiveConnections.Clear();
			currentVisibleIndex = 0;
			ResetTimer();
			
			if(nodeController.SelectedNode == null) return;
			foreach (var connection in GetActiveNodeConnections()) {
				GameObject connectionObject = Connections.Pool.Spawn();
				var childObj = GraphController.Graph.NodeObjectMap[connection];
				InitializeConnection(ref connectionObject, GraphController.Graph.NodeObjectMap[nodeController.SelectedNode], childObj);
				ActiveConnections.Add(connection);
				ConnectionObjectMap.Add(connection, connectionObject);
			}
			UpdateVisibleConnections();
		}

		private List<Node> GetActiveNodeConnections() {
			if (nodeController.SelectedNode == null) return null;
			return nodeController.SelectedNode.GetConnections(graphController.ConnectionMode.Value).Select(id => {
				nodeController.LoadNode(id); //TODO handle loading in separate controller
				return GraphController.Graph.IdNodeMap[id];
			}).ToList();
		}

		private void InitializeConnection(ref GameObject connectionObject, GameObject from, GameObject to) {
			var basePosition = from.transform.position;
			connectionObject.SetActive(false);
			connectionObject.name = to.name;
			connectionObject.transform.position = basePosition;
			connectionObject.transform.parent = Connections.Container.transform;
			
			var line = connectionObject.GetComponent<LineRenderer>();
			line.material.color = graphController.ConnectionMode.Value == ConnectionMode.PARENTS ? ParentConnectionColor : ChildConnectionColor;
			
			Connection connectionModel = connectionService.GenerateConnection(basePosition, to.transform.position);
			line.positionCount = connectionModel.SegmentPoints.Length;
			line.SetPositions(connectionModel.SegmentPoints);
		}
		
		private void ResetTimer() {
			scrollTimer = ScrollInterval * 1000;
		}

		#region Mono Behaviour

		private NodeController nodeController;
		private GraphController graphController;
		private NetworkController networkController;
		
		void Awake() {
			graphController = GetComponent<GraphController>();
			nodeController = GetComponent<NodeController>();
			networkController = GetComponent<NetworkController>();
		}

		private void Start() {
			Connections.Pool = new GameObjectPool(Connections.Prefab, Connections.PreloadNumber, Connections.PoolContainer);
			if (networkController.IsServer()) {
				nodeController.OnSelectedNodeChanged += mode => LoadConnections();
				graphController.ConnectionMode.OnValueChanged += mode => LoadConnections();
			}
			connectionService = new ConnectionService(); 
		}

		private void Update() {
			if (scrollDirection != 0) {
				scrollTimer -= Time.deltaTime * 1000;
				if (scrollTimer <= 0) {
					UpdateVisibleConnections();
					ResetTimer();
				}
			}
		}

		#endregion
	}
}