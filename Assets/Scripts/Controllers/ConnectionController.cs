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
				NetworkController.SyncConnectionScrolled(scrollDirection);
				ResetTimer();
			}
		}

		private void OnConnectionNodeChanged(Node oldNode, Node newNode) {
			graph.ConnectionObjectMap.Keys.ToList().ForEach(ConnectionManager.UnloadConnection);

			currentVisibleIndex = 0;
			ResetTimer();

			if (newNode == null) return;
			UpdateVisibleConnections();
			SwitchConnectionTypes();
		}

		private void OnHighlightedNodeChanged(Node oldNode, Node newNode) {
			if(NodeController.SelectedNode != null && (NodeController.SelectedNode == oldNode || NodeController.SelectedNode == newNode)) 
				return;
			if(oldNode != null) 
				GetNodeConnections(oldNode).Where(connection => !connection.Ends.Contains(NodeController.SelectedNode)).ToList().ForEach(ConnectionManager.UnloadConnection);
			if(newNode != null)
				CreateNodeConnections(newNode).ForEach(ConnectionManager.LoadConnection);
		}
		
		#endregion

		#region Connection Updating
		
		private void UnloadNodeConnections(Node centerNode) {
			graph.ConnectionObjectMap.Keys.Where(connection => connection.Ends.Contains(centerNode)).ToList().ForEach(ConnectionManager.UnloadConnection);
		}

		public void UpdateVisibleConnections(int direction = 0) {
			if (direction == 0)
				direction = scrollDirection;
			var connections = CreateNodeConnections(NodeController.SelectedNode);
			if (connections.Count <= MaxVisibleConnections) {
				connections.ForEach(ConnectionManager.LoadConnection);
				return;
			}
			UnloadNodeConnections(NodeController.SelectedNode);

			currentVisibleIndex = Utils.Mod(currentVisibleIndex + direction * ChangeConnectionNumber, connections.Count);
			Utils.GetCircularListPart(connections, currentVisibleIndex, MaxVisibleConnections)
				.ForEach(ConnectionManager.LoadConnection);
		}

		private void SwitchConnectionTypes() {
			GetNodeConnections(NodeController.SelectedNode).ForEach(ConnectionManager.SetConnectionLineColor);
		}

		private List<Connection> CreateNodeConnections(Node node) {
			if (node == null) return null;
			var connections = node.GetConnections(GraphController.ConnectionMode.Value).Where(id => id != node.ID).Select(id => {
				NodeController.LoadNode(id);
				return new Connection(node, graph.IdNodeMap[id]);
			}).ToList();
			NodeController.OnNodeLoadSessionEnded?.Invoke(); //can trigger loading of unloaded connected nodes TODO move once we have a node loader
			return connections;
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
			
			NodeController.OnSelectedNodeChanged += OnConnectionNodeChanged;
			NodeController.OnHighlightedNodeChanged += OnHighlightedNodeChanged;
			GraphController.ConnectionMode.OnValueChanged += mode => OnConnectionNodeChanged(NodeController.SelectedNode, NodeController.SelectedNode);

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