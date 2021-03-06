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
		public Action<int> OnScrollInDirection;

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
			if(direction == 0)
				OnScrollInDirection?.Invoke(0);
			if(GraphController.GraphMode.Value == GraphMode.FREE_FLIGHT)
				return;
			scrollDirection = direction;
			SeriesScrolls = 0;
		}

		public void OnAdvanceScrollInput() {
			if (scrollDirection == 0) return;
			scrollTimer -= Time.deltaTime * 1000;
			if (scrollTimer <= 0) {
				LoadNewConnectedNodeSet(NodeController.SelectedNode, FilterSelectedNodeSet);
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
			UpdateVisibleConnections(scrollDirection, true);
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

		#region Node Loading Requests

		private void LoadNewConnectedNodeSet(Node centerNode, Func<IEnumerable<uint>, List<uint>> setFilter, bool resetIndex = false, ConnectionMode? mode = null) {
			if(resetIndex)
				currentVisibleIndex = 0;
			if(centerNode == null)
				return;
			setFilter(GetNodeNeighbours(centerNode, mode ?? GraphController.ConnectionMode.Value)).ForEach(id => NodeController.NodeLoadManager.LoadNode(id));
			NodeController.OnNodeLoadSessionEnded?.Invoke();
		}

		private List<uint> FilterHighlightedNodeSet(IEnumerable<uint> set) => set.Take(ConnectionDistribution.MaxVisibleNumber).ToList();

		private List<uint> FilterSelectedNodeSet(IEnumerable<uint> set) {
			var connectedIDs = set.ToList();
			List<uint> newSubList;
			if (connectedIDs.Count <= ConnectionDistribution.MaxVisibleNumber) {
				newSubList = connectedIDs;
			} else {
				var index = currentVisibleIndex;
				newSubList = Utils.ScrollList(connectedIDs, ref index, scrollDirection * ConnectionDistribution.ChangeBy,
					ConnectionDistribution.MaxVisibleNumber);
			}
			return newSubList;
		}

		#endregion

		#region Connection Updating

		public void UpdateVisibleConnections(int direction, bool centralNodeChanged = false) {
			var centerNode = NodeController.SelectedNode;
			var connectedIDs = GetNodeNeighbours(centerNode).ToList();
			
			if (connectedIDs.Count <= ConnectionDistribution.MaxVisibleNumber) {
				connectedIDs.ForEach(id => ConnectionLoadManager.LoadConnection(CreateConnection(centerNode, id), selectedNodeDistribution));
				OnConnectionRangeChanged?.Invoke(Mathf.Min(1, connectedIDs.Count), connectedIDs.Count, connectedIDs.Count);
			} else {
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
			if(!centralNodeChanged)
				OnScrollInDirection?.Invoke(direction);
		}

		private void SwitchConnectionTypes() {
			GetConnectionsAround(NodeController.SelectedNode).ForEach(ConnectionLoadManager.SetConnectionLineColor);
		}

		private List<Connection> CreateConnectionsAround(Node centerNode, int limit) {
			return GetNodeNeighbours(centerNode).Take(limit).Select(id => CreateConnection(centerNode, id)).ToList();
		}

		private Connection CreateConnection(Node centerNode, uint otherID) {
			if (!GraphController.Graph.IdNodeMap.ContainsKey(otherID)) {
				Debug.LogWarning("Node needed by connection not loaded");
				return null;
			}
			return new Connection(centerNode, GraphController.Graph.IdNodeMap[otherID]);
		}

		public IEnumerable<uint> GetNodeNeighbours(Node centerNode) {
			return GetNodeNeighbours(centerNode, GraphController.ConnectionMode.Value);
		}

		public IEnumerable<uint> GetNodeNeighbours(Node centerNode, ConnectionMode mode) {
			return centerNode.GetConnections(mode).Where(id => id != centerNode.ID);
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

			if (NetworkController.IsServer()) {
				NetworkController.BeforeConnectionModeSync += mode => LoadNewConnectedNodeSet(NodeController.SelectedNode, FilterSelectedNodeSet, true, mode);
				NetworkController.BeforeSelectedNodeSync += node => LoadNewConnectedNodeSet(node, FilterSelectedNodeSet, true);
				NetworkController.BeforeHighlightedNodeSync += node => LoadNewConnectedNodeSet(node, FilterHighlightedNodeSet);
			}
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