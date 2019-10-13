using System;
using System.Collections.Generic;
using System.Linq;
using Model;
using Model.Connection;
using Services;
using Services.Connection;
using Services.ObjectPool;
using UnityEngine;

namespace Controllers {
	public class ConnectionController : MonoBehaviour {
		public GraphPooledObject Connections;
		public ConnectionColors Colors;
		
		public ConnectionDistribution ConnectionDistribution;
		public float ScrollInterval;

		public Action<Connection> OnConnectionLoaded;
		public Action<Connection> OnConnectionUnloaded;
		
		public GameObject ConnectionMarker; //temp

		public ConnectionManager ConnectionManager { get; private set; }
		private ConnectionDistributionService selectedNodeDistribution;
		private ConnectionDistributionService highlightedNodeDistribution;
		private Graph graph => GraphController.Graph;

		private int currentVisibleIndex;
		private int scrollDirection;
		private float scrollTimer;

		#region Callbacks

		#region Scrolling

		public void OnScrollInputChanged(int direction) {
			scrollDirection = direction;
		}

		public void OnAdvanceScrollInput(int direction) {
			OnScrollInputChanged(direction);
			if (scrollDirection == 0) return;
			scrollTimer -= Time.deltaTime * 1000;
			if (scrollTimer <= 0) {
				NetworkController.SyncConnectionScrolled(scrollDirection);
				ResetTimer();
			}
		}

		private void ReloadSelectedNodeConnections(Node centerNode) {
			graph.ConnectionObjectMap.Keys.ToList().ForEach(ConnectionManager.UnloadConnection);

			ResetTimer();

			if (centerNode == null) return;
			selectedNodeDistribution = new ConnectionDistributionService(centerNode, this);
			UpdateVisibleConnections(scrollDirection);
			SwitchConnectionTypes();
		}

		private void ResetTimer() {
			scrollTimer = ScrollInterval * 1000;
		}
		
		#endregion

		private void OnHighlightedNodeChanged(Node oldNode, Node newNode) {
			if(NodeController.SelectedNode != null && (NodeController.SelectedNode == oldNode || NodeController.SelectedNode == newNode)) 
				return;
			if(oldNode != null) 
				GetConnectionsAround(oldNode).Where(connection => !connection.Ends.Contains(NodeController.SelectedNode)).ToList().ForEach(ConnectionManager.UnloadConnection);
			if(newNode != null) {
				highlightedNodeDistribution = new ConnectionDistributionService(newNode, this);
				CreateConnectionsAround(newNode, ConnectionDistribution.MaxVisibleConnections).ToList().ForEach(con => ConnectionManager.LoadConnection(con, highlightedNodeDistribution));
			}
		}
		
		#endregion

		#region Connection Updating
		
		private void UnloadNodeConnections(Node centerNode) {
			graph.ConnectionObjectMap.Keys.Where(connection => connection.Ends.Contains(centerNode)).ToList().ForEach(ConnectionManager.UnloadConnection);
		}

		public void UpdateVisibleConnections(int direction) {
			var connections = CreateConnectionsAround(NodeController.SelectedNode);
			if (connections.Count <= ConnectionDistribution.MaxVisibleConnections) {
				connections.ForEach(con => ConnectionManager.LoadConnection(con, selectedNodeDistribution));
				return;
			}
			var oldSubList = GetConnectionsAround(NodeController.SelectedNode);

			currentVisibleIndex = Utils.Mod(currentVisibleIndex + direction * ConnectionDistribution.ChangeConnectionNumber, connections.Count);
			var newSubList = Utils.GetCircularListPart(connections, currentVisibleIndex, ConnectionDistribution.MaxVisibleConnections);
			oldSubList.Where(connection => !newSubList.Contains(connection)).ToList().ForEach(connection => {
				selectedNodeDistribution.OnConnectionUnloaded(connection);
				ConnectionManager.UnloadConnection(connection);
			});
			newSubList.Where(connection => !oldSubList.Contains(connection)).ToList().ForEach(con => ConnectionManager.LoadConnection(con, selectedNodeDistribution));
		}

		private void SwitchConnectionTypes() {
			GetConnectionsAround(NodeController.SelectedNode).ForEach(ConnectionManager.SetConnectionLineColor);
		}

		private List<Connection> CreateConnectionsAround(Node centerNode, int limit = -1) {
			if (centerNode == null) return null;
			var enumerable = GetNodeNeighbours(centerNode);
			if (limit >= 0)
				enumerable = enumerable.Take(limit);
			var connections = enumerable.Select(id => new Connection(centerNode, NodeController.LoadNode(id))).ToList();
			NodeController.OnNodeLoadSessionEnded?.Invoke(); //can trigger loading of unloaded connected nodes TODO move once we have a node loader
			return connections;
		}

		public IEnumerable<uint> GetNodeNeighbours(Node centerNode) {
			return centerNode.GetConnections(GraphController.ConnectionMode.Value).Where(id => id != centerNode.ID);
		}

		private List<Connection> GetConnectionsAround(Node centerNode) {
			return graph.ConnectionObjectMap.Keys.ToList().Where(connection => connection.Ends.Contains(centerNode)).ToList();
		}

		#endregion

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
			
			NodeController.OnSelectedNodeChanged += (oldNode, newNode) => {
				currentVisibleIndex = 0;
				ReloadSelectedNodeConnections(newNode);
			};
			GraphController.ConnectionMode.OnValueChanged += mode => ReloadSelectedNodeConnections(NodeController.SelectedNode);
			NodeController.OnHighlightedNodeChanged += OnHighlightedNodeChanged;

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