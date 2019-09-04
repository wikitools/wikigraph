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

		public Action<Node> OnConnectionLoaded;
		public Action<Node> OnConnectionUnloaded;
		public Action OnConnectionLoadSessionEnded;
		
		private ConnectionService connectionService;
		
		private Dictionary<Node, GameObject> ConnectionObjectMap = new Dictionary<Node, GameObject>();
		
		private int currentVisibleIndex;
		private int scrollDirection;
		private float scrollTimer;

		public void OnScrollInputChanged(int direction) {
			scrollDirection = direction;
		}

		private void ScrollConnections() {
			UnloadAll();
			OnConnectionLoadSessionEnded?.Invoke();

			var connections = GetNodeConnections(nodeController.SelectedNode);
			currentVisibleIndex = Utils.Mod(currentVisibleIndex + scrollDirection * ChangeConnectionNumber, connections.Count);
			Utils.GetCircularListPart(connections, currentVisibleIndex, MaxVisibleConnections).ForEach(networkController.SyncLoadedConnection);
			OnConnectionLoadSessionEnded?.Invoke();
		}

		public void LoadConnection(Node node) {
			InitConnection(nodeController.SelectedNode, node);
			OnConnectionLoaded?.Invoke(node);
		}

		public void UnloadConnection(Node node) {
			if(!ConnectionObjectMap.ContainsKey(node))
				return;
			Connections.Pool.Despawn(ConnectionObjectMap[node]);
			ConnectionObjectMap.Remove(node);
			OnConnectionUnloaded?.Invoke(node);
		}

		private void UnloadAll() {
			ConnectionObjectMap.Keys.ToList().ForEach(networkController.SyncUnloadedConnection);
		}

		private void InitNodeConnections(Node centerNode) {
			UnloadAll();
			currentVisibleIndex = 0;
			ResetTimer();
			
			if(nodeController.SelectedNode == null) return;
			ScrollConnections();
		}

		private void InitConnection(Node from, Node to) {
			GameObject connectionObject = Connections.Pool.Spawn();
			InitConnectionObject(ref connectionObject, GraphController.Graph.NodeObjectMap[from], GraphController.Graph.NodeObjectMap[to]);
			ConnectionObjectMap.Add(to, connectionObject);
		}

		private List<Node> GetNodeConnections(Node node) {
			if (node == null) return null;
			return node.GetConnections(graphController.ConnectionMode.Value).Select(id => {
				nodeController.LoadNode(id); //TODO handle loading in separate controller
				return GraphController.Graph.IdNodeMap[id];
			}).ToList();
		}

		private void InitConnectionObject(ref GameObject connectionObject, GameObject from, GameObject to) {
			var basePosition = from.transform.position;
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
			nodeController.OnSelectedNodeChanged += InitNodeConnections;
			graphController.ConnectionMode.OnValueChanged += mode => InitNodeConnections(nodeController.SelectedNode);
			connectionService = new ConnectionService(); 
		}

		private void Update() {
			if (scrollDirection != 0) {
				scrollTimer -= Time.deltaTime * 1000;
				if (scrollTimer <= 0) {
					ScrollConnections();
					ResetTimer();
				}
			}
		}

		#endregion
	}
}