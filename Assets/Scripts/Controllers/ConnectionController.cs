using System;
using System.Collections.Generic;
using System.Linq;
using Model;
using Services;
using Services.Connection;
using Services.ObjectPool;
using UnityEngine;

namespace Controllers {
	public class ConnectionController : MonoBehaviour {
		private readonly Logger<ConnectionController> logger = new Logger<ConnectionController>();

		public GraphPooledObject Connections;
		public int MaxVisibleConnections;
		public int ChangeConnectionNumber;
		public float ScrollInterval;

		public Color ChildConnectionColor;
		public Color ParentConnectionColor;

		public Action<Connection> OnConnectionLoaded;
		public Action<Connection> OnConnectionUnloaded;
		public Action OnConnectionLoadSessionEnded;

		private ConnectionService connectionService;
		private List<Tuple<uint, uint>> connectionQueue = new List<Tuple<uint, uint>>();
		private Graph graph => GraphController.Graph;

		private int currentVisibleIndex;
		private int scrollDirection;
		private float scrollTimer;

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
			OnConnectionLoaded?.Invoke(connection);
		}

		public void UnloadConnection(Tuple<uint, uint> nodeIDs) {
			var connection = CreateConnection(nodeIDs);

			if (!graph.ConnectionObjectMap.ContainsKey(connection))
				return;
			Connections.Pool.Despawn(graph.ConnectionObjectMap[connection]);
			graph.ConnectionObjectMap.Remove(connection);
			OnConnectionUnloaded?.Invoke(connection);
		}

		private void UnloadAllConnections() {
			graph.ConnectionObjectMap.Keys.ToList().ForEach(SyncUnloadedConnection);
		}

		#endregion

		#region Callbacks

		public void OnScrollInputChanged(int direction) {
			scrollDirection = direction;
		}

		public void OnAdvanceScrollInput(int direction) {
			OnScrollInputChanged(direction);
			if (scrollDirection == 0) return;
			scrollTimer -= Time.deltaTime * 1000;
			if (scrollTimer <= 0) {
				UpdateVisibleConnections();
				ResetTimer();
			}
		}

		private void OnConnectionNodeChanged(Node centerNode) {
			UnloadAllConnections();
			OnConnectionLoadSessionEnded?.Invoke();

			currentVisibleIndex = 0;
			ResetTimer();

			if (centerNode == null) return;
			UpdateVisibleConnections();
		}

		#endregion

		#region Connection Updating

		private void SyncLoadedConnection(Connection connection) {
			if (!graph.ConnectionObjectMap.ContainsKey(connection))
				networkController.SyncLoadedConnection(connection.AsTuple());
		}

		private void SyncUnloadedConnection(Connection connection) {
			if (graph.ConnectionObjectMap.ContainsKey(connection))
				networkController.SyncUnloadedConnection(connection.AsTuple());
		}

		private void UpdateVisibleConnections() {
			var connections = GetNodeConnections(nodeController.SelectedNode);
			nodeController.OnNodeLoadSessionEnded?.Invoke(); //can trigger loading of unloaded connected nodes TODO move once we have a node loader
			if (connections.Count <= MaxVisibleConnections) {
				connections.ForEach(SyncLoadedConnection);
				OnConnectionLoadSessionEnded?.Invoke();
				return;
			}
			UnloadAllConnections();
			OnConnectionLoadSessionEnded?.Invoke();

			currentVisibleIndex = Utils.Mod(currentVisibleIndex + scrollDirection * ChangeConnectionNumber, connections.Count);
			Utils.GetCircularListPart(connections, currentVisibleIndex, MaxVisibleConnections)
				.ForEach(SyncLoadedConnection);
			OnConnectionLoadSessionEnded?.Invoke();
		}

		private void SwitchConnectionTypes() {
			GetSelectedNodeConnections().ForEach(connection => SetConnectionLineColor(graph.ConnectionObjectMap[connection].GetComponent<LineRenderer>()));
		}

		private List<Connection> GetNodeConnections(Node node) {
			if (node == null) return null;
			return node.GetConnections(graphController.ConnectionMode.Value).Select(id => {
				nodeController.LoadNode(id);
				return new Connection(node, graph.IdNodeMap[id]);
			}).ToList();
		}

		private List<Connection> GetSelectedNodeConnections() {
			return graph.ConnectionObjectMap.Keys.ToList().Where(connection => connection.Ends.Contains(nodeController.SelectedNode)).ToList();
		}

		#endregion

		#region Connection Creation

		private Connection CreateConnection(Tuple<uint, uint> connection) {
			return new Connection(graph.IdNodeMap[connection.Item1], graph.IdNodeMap[connection.Item2]);
		}

		private bool CanLoadConnection(Tuple<uint, uint> connection) =>
			graph.IdNodeMap.ContainsKey(connection.Item1) && graph.IdNodeMap.ContainsKey(connection.Item2);

		private void CheckConnectionQueue() {
			for (var i = connectionQueue.Count - 1; i >= 0; i--) {
				if (CanLoadConnection(connectionQueue[i])) {
					DoLoadConnection(connectionQueue[i]);
					connectionQueue.RemoveAt(i);
				}
			}
		}

		private void InitConnection(Connection connection) {
			if (!connection.Item1.GetConnections(graphController.ConnectionMode.Value).Contains(connection.Item2.ID) && networkController.IsServer())
				logger.Warning("Attempting to create connection that does not exist");

			GameObject connectionObject = Connections.Pool.Spawn();
			connection.Route = InitConnectionObject(ref connectionObject, graph.NodeObjectMap[connection.Item1], graph.NodeObjectMap[connection.Item2]);
			graph.ConnectionObjectMap.Add(connection, connectionObject);
		}

		private Route InitConnectionObject(ref GameObject connectionObject, GameObject from, GameObject to) {
			var basePosition = from.transform.position;
			connectionObject.name = to.name;
			connectionObject.transform.position = basePosition;
			connectionObject.transform.parent = Connections.Container.transform;

			var line = connectionObject.GetComponent<LineRenderer>();
			SetConnectionLineColor(line);

			Route route = connectionService.GenerateConnection(basePosition, to.transform.position);
			line.positionCount = route.SegmentPoints.Length;
			line.SetPositions(route.SegmentPoints);
			return route;
		}

		private void SetConnectionLineColor(LineRenderer line) {
			line.material.color = graphController.ConnectionMode.Value == ConnectionMode.PARENTS ? ParentConnectionColor : ChildConnectionColor;
		}

		#endregion

		private void ResetTimer() {
			scrollTimer = ScrollInterval * 1000;
		}

		#region Mono Behaviour

		private NodeController nodeController;
		private GraphController graphController;
		private NetworkController networkController;
		private  InputController inputController;

		void Awake() {
			graphController = GetComponent<GraphController>();
			nodeController = GetComponent<NodeController>();
			networkController = GetComponent<NetworkController>();
			inputController = GetComponent<InputController>();
		}

		private void Start() {
			Connections.Pool = new GameObjectPool(Connections.Prefab, Connections.PreloadNumber, Connections.PoolContainer);
			if (networkController.IsServer()) {
				nodeController.OnSelectedNodeChanged += (oldNode, newNode) => OnConnectionNodeChanged(newNode);
				graphController.ConnectionMode.OnValueChanged += mode => OnConnectionNodeChanged(nodeController.SelectedNode);
			} else {
				graphController.ConnectionMode.OnValueChanged += mode => SwitchConnectionTypes();
				nodeController.OnNodeLoaded += (node, pos) => CheckConnectionQueue();
			}
			connectionService = new ConnectionService();
		}

		private void Update() {
			if (networkController.IsServer() && inputController.Environment == Environment.PC)
				OnAdvanceScrollInput(scrollDirection);
		}

		#endregion
	}
}