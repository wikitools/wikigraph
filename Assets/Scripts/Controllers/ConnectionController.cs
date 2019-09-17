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
		public GraphPooledObject Connections;
		public ConnectionColors Colors;
		
		public int MaxVisibleConnections;
		public int ChangeConnectionNumber;
		public float ScrollInterval;

		public Action<Connection> OnConnectionLoaded;
		public Action<Connection> OnConnectionUnloaded;
		public Action OnConnectionLoadSessionEnded;

		public ConnectionManager ConnectionManager { get; private set; }
		private Graph graph => GraphController.Graph;

		private int currentVisibleIndex;
		private int scrollDirection;
		private float scrollTimer;

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

		private void OnHighlightedNodeChanged(Node node) {
			if(nodeController.SelectedNode == null || nodeController.SelectedNode == node) 
				return;
			//if(node != null)
				
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
		
		private void UnloadAllConnections() {
			graph.ConnectionObjectMap.Keys.ToList().ForEach(SyncUnloadedConnection);
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
			GetSelectedNodeConnections().ForEach(ConnectionManager.SetConnectionLineColor);
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
				nodeController.OnHighlightedNodeChanged += OnHighlightedNodeChanged;
				graphController.ConnectionMode.OnValueChanged += mode => OnConnectionNodeChanged(nodeController.SelectedNode);
			} else {
				graphController.ConnectionMode.OnValueChanged += mode => SwitchConnectionTypes();
				nodeController.OnNodeLoaded += (node, pos) => ConnectionManager.CheckConnectionQueue();
			}
			ConnectionManager = new ConnectionManager(this, graphController.ConnectionMode, networkController.IsServer);
		}

		private void Update() {
			if (networkController.IsServer() && inputController.Environment == Environment.PC)
				OnAdvanceScrollInput(scrollDirection);
		}

		#endregion
		
		[Serializable]
		public class ConnectionColors {
			public Color ChildColor;
			public Color ParentColor;
			public Color DisabledColor;
		}
	}
}