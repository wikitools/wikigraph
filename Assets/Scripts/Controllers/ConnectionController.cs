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

		private void OnConnectionNodeChanged(Node oldNode, Node newNode) {
			UnloadNodeConnections(oldNode);
			OnConnectionLoadSessionEnded?.Invoke();

			currentVisibleIndex = 0;
			ResetTimer();

			if (newNode == null) return;
			UpdateVisibleConnections();
		}

		private void OnHighlightedNodeChanged(Node oldNode, Node newNode) {
			if(NodeController.SelectedNode == null || NodeController.SelectedNode == oldNode || NodeController.SelectedNode == newNode) 
				return;
			if(oldNode != null) 
				GetNodeConnections(oldNode).Where(connection => !connection.Ends.Contains(NodeController.SelectedNode)).ToList().ForEach(SyncUnloadedConnection);
			if(newNode != null)
				CreateNodeConnections(newNode).ForEach(SyncLoadedConnection);
			OnConnectionLoadSessionEnded?.Invoke();
		}
		
		#endregion

		#region Connection Updating

		private void SyncLoadedConnection(Connection connection) {
			if (!graph.ConnectionObjectMap.ContainsKey(connection))
				NetworkController.SyncLoadedConnection(connection.AsTuple());
		}

		private void SyncUnloadedConnection(Connection connection) {
			if (graph.ConnectionObjectMap.ContainsKey(connection))
				NetworkController.SyncUnloadedConnection(connection.AsTuple());
		}
		
		private void UnloadNodeConnections(Node centerNode) {
			graph.ConnectionObjectMap.Keys.Where(connection => connection.Ends.Contains(centerNode)).ToList().ForEach(SyncUnloadedConnection);
		}

		private void UpdateVisibleConnections() {
			var connections = CreateNodeConnections(NodeController.SelectedNode);
			NodeController.OnNodeLoadSessionEnded?.Invoke(); //can trigger loading of unloaded connected nodes TODO move once we have a node loader
			if (connections.Count <= MaxVisibleConnections) {
				connections.ForEach(SyncLoadedConnection);
				OnConnectionLoadSessionEnded?.Invoke();
				return;
			}
			UnloadNodeConnections(NodeController.SelectedNode);
			OnConnectionLoadSessionEnded?.Invoke();

			currentVisibleIndex = Utils.Mod(currentVisibleIndex + scrollDirection * ChangeConnectionNumber, connections.Count);
			Utils.GetCircularListPart(connections, currentVisibleIndex, MaxVisibleConnections)
				.ForEach(SyncLoadedConnection);
			OnConnectionLoadSessionEnded?.Invoke();
		}

		private void SwitchConnectionTypes() {
			GetNodeConnections(NodeController.SelectedNode).ForEach(ConnectionManager.SetConnectionLineColor);
		}

		private List<Connection> CreateNodeConnections(Node node) {
			if (node == null) return null;
			return node.GetConnections(GraphController.ConnectionMode.Value).Select(id => {
				NodeController.LoadNode(id);
				return new Connection(node, graph.IdNodeMap[id]);
			}).ToList();
		}

		private List<Connection> GetNodeConnections(Node node) {
			return graph.ConnectionObjectMap.Keys.ToList().Where(connection => connection.Ends.Contains(node)).ToList();
		}

		#endregion

		private void ResetTimer() {
			scrollTimer = ScrollInterval * 1000;
		}

		#region Mono Behaviour

		public NodeController NodeController { get; private set; }
		public GraphController GraphController { get; private set; }
		public NetworkController NetworkController { get; private set; }
		public InputController InputController { get; private set; }

		void Awake() {
			GraphController = GetComponent<GraphController>();
			NodeController = GetComponent<NodeController>();
			NetworkController = GetComponent<NetworkController>();
			InputController = GetComponent<InputController>();
		}

		private void Start() {
			Connections.Pool = new GameObjectPool(Connections.Prefab, Connections.PreloadNumber, Connections.PoolContainer);
			if (NetworkController.IsServer()) {
				NodeController.OnSelectedNodeChanged += OnConnectionNodeChanged;
				NodeController.OnHighlightedNodeChanged += OnHighlightedNodeChanged;
				GraphController.ConnectionMode.OnValueChanged += mode => OnConnectionNodeChanged(NodeController.SelectedNode, NodeController.SelectedNode);
			} else {
				GraphController.ConnectionMode.OnValueChanged += mode => SwitchConnectionTypes();
				NodeController.OnNodeLoaded += (node, pos) => ConnectionManager.CheckConnectionQueue();
			}
			ConnectionManager = new ConnectionManager(this);
		}

		private void Update() {
			if (NetworkController.IsServer() && InputController.Environment == Environment.PC)
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