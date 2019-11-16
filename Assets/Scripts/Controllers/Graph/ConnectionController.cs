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
		public Vector2 ScrollInterval;
		public Vector2 ScrollAcceleration;
		private int SeriesScrolls;
		[Range(1, 6)]
		public int ConnectionLoadSpeed = 4;

		public Action<Connection> OnConnectionLoaded;
		public Action<Connection> OnConnectionUnloaded;
		public Action<int, int, int> OnConnectionRangeChanged;

		public ConnectionLoadManager ConnectionLoadManager { get; private set; }
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
			SeriesScrolls = 0;
		}

		public void OnAdvanceScrollInput() {
			if (scrollDirection == 0) return;
			scrollTimer -= Time.deltaTime * 1000;
			if (scrollTimer <= 0) {
				NetworkController.SyncConnectionScrolled(scrollDirection);
				SeriesScrolls++;
				ResetTimer();
			}
		}

		private void ReloadSelectedNodeConnections(Node centerNode) {
			currentVisibleIndex = 0;
			graph.ConnectionObjectMap.Keys.ToList().ForEach(ConnectionLoadManager.UnloadConnection);

			SeriesScrolls = 0;
			ResetTimer();

			if (centerNode == null) return;
			selectedNodeDistribution = new ConnectionDistributionService(centerNode, this);
			UpdateVisibleConnections(scrollDirection);
			SwitchConnectionTypes();
		}

		private void ResetTimer() {
			var acceleration = Mathf.Max(0, Mathf.Min(SeriesScrolls, ScrollAcceleration.y) - ScrollAcceleration.x);
			scrollTimer = (ScrollInterval.x - (ScrollInterval.x - ScrollInterval.y) * acceleration / (ScrollAcceleration.y - ScrollAcceleration.x)) * 1000;
		}
		
		#endregion

		private void OnHighlightedNodeChanged(Node oldNode, Node newNode) {
			if(NodeController.SelectedNode != null && (NodeController.SelectedNode == oldNode || NodeController.SelectedNode == newNode)) 
				return;
			if(oldNode != null) 
				GetConnectionsAround(oldNode).Where(connection => !connection.Ends.Contains(NodeController.SelectedNode)).ToList().ForEach(ConnectionLoadManager.UnloadConnection);
			if(newNode != null) {
				highlightedNodeDistribution = new ConnectionDistributionService(newNode, this);
				CreateConnectionsAround(newNode, ConnectionDistribution.MaxVisibleNumber).ToList().ForEach(con => ConnectionLoadManager.LoadConnection(con, highlightedNodeDistribution));
			}
		}
		
		#endregion

		#region Connection Updating

		public void UpdateVisibleConnections(int direction) {
			var centerNode = NodeController.SelectedNode;
			var connectedIDs = GetNodeNeighbours(centerNode).ToList();
			if (connectedIDs.Count <= ConnectionDistribution.MaxVisibleNumber) {
				connectedIDs.ForEach(id => ConnectionLoadManager.LoadConnection(CreateConnection(centerNode, id), selectedNodeDistribution));
				OnConnectionRangeChanged?.Invoke(Mathf.Min(1, connectedIDs.Count), connectedIDs.Count, connectedIDs.Count);
				return;
			}
			var oldSubList = GetConnectionsAround(centerNode);
			var oldIDList = oldSubList.Select(con => con.OtherEnd(centerNode).ID).ToList();

			var newSubList = Utils.ScrollList(connectedIDs, ref currentVisibleIndex, 
				direction * ConnectionDistribution.ChangeBy, ConnectionDistribution.MaxVisibleNumber);
			newSubList.Where(id => !oldIDList.Contains(id)).ToList().ForEach(id => 
				ConnectionLoadManager.LoadConnection(CreateConnection(centerNode, id), selectedNodeDistribution));
			oldSubList.Where(connection => !newSubList.Contains(connection.OtherEnd(centerNode).ID)).ToList().ForEach(connection => {
				selectedNodeDistribution.OnConnectionUnloaded(connection);
				ConnectionLoadManager.UnloadConnection(connection);
			});
			int endIndex = Utils.Mod(currentVisibleIndex + ConnectionDistribution.MaxVisibleNumber - 1, connectedIDs.Count);
			OnConnectionRangeChanged?.Invoke(currentVisibleIndex + 1, endIndex + 1, connectedIDs.Count);
		}

		private void SwitchConnectionTypes() {
			GetConnectionsAround(NodeController.SelectedNode).ForEach(ConnectionLoadManager.SetConnectionLineColor);
		}

		private List<Connection> CreateConnectionsAround(Node centerNode, int limit) {
			var connections = GetNodeNeighbours(centerNode).Take(limit).Select(id => new Connection(centerNode, NodeController.NodeLoadManager.LoadNode(id))).ToList();
			NodeController.OnNodeLoadSessionEnded?.Invoke(); //can trigger loading of unloaded connected nodes TODO move once we have a node loader
			return connections;
		}

		private Connection CreateConnection(Node centerNode, uint otherID) {
			return new Connection(centerNode, NodeController.NodeLoadManager.LoadNode(otherID));
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
			
			NodeController.OnSelectedNodeChanged += (oldNode, newNode) => ReloadSelectedNodeConnections(newNode);
			GraphController.ConnectionMode.OnValueChanged += mode => ReloadSelectedNodeConnections(NodeController.SelectedNode);
			NodeController.OnHighlightedNodeChanged += OnHighlightedNodeChanged;

			ConnectionLoadManager = new ConnectionLoadManager(this);
		}

		private void Update() {
			if (NetworkController.IsServer())
				OnAdvanceScrollInput();
		}

		#endregion
		
		[Serializable]
		public class ConnectionColors {
			public Color ChildColor;
			public Color ParentColor;
			public Color CategoryColor;
			public Color DisabledColor;
		}
	}
}