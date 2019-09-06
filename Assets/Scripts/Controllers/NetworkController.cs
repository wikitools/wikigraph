using System;
using Model;
using Services.SyncBuffer;
using UnityEngine;

#pragma warning disable 618
namespace Controllers {
	public class NetworkController: MonoBehaviour {
		private readonly RPCMode RPC_MODE = RPCMode.AllBuffered;
		
		private NetworkView NetworkView;
		private NodeSyncBuffer nodeSyncBuffer;
		private ConnectionSyncBuffer connectionSyncBuffer;

		private static readonly int PORT = 25000;

		private Environment Environment => inputController.Environment;

		#region RPC Sync

		public void SyncLoadedConnection(Tuple<uint, uint> connection) => connectionSyncBuffer.OnConnectionLoaded(connection.Item1, connection.Item2);
		
		public void SyncUnloadedConnection(Tuple<uint, uint> connection) => connectionSyncBuffer.OnConnectionUnloaded(connection.Item1, connection.Item2);
		
		[RPC]
		private void syncConnection(string stream, bool loaded) {
			ConnectionSyncBuffer.ParseConnections(stream).ForEach(connection => {
				if(loaded)
					connectionController.LoadConnection(connection);
				else
					connectionController.UnloadConnection(connection);
			});
		}
		
		private void SyncLoadedNodes(string stream) => Synchronize("syncNodes", stream, true);
		
		private void SyncUnloadedNodes(string stream) => Synchronize("syncNodes", stream, false);
		
		[RPC]
		private void syncNodes(string stream, bool loaded) {
			if(loaded) //TODO add node unloading sync once we support it
				NodeSyncBuffer.ParseLoadedNodes(stream).ForEach(node => nodeController.LoadNode(node.ID, node.Position));
		}
		
		public void SetGraphMode(GraphMode mode) => Synchronize("setGraphMode", (int) mode);
		
		[RPC]
		private void setGraphMode(int value) {
			graphController.GraphMode.Value = (GraphMode) value;
		}
		
		public void SetHighlightedNode(string id) => Synchronize("setHighlightedNode", id);
		
		[RPC]
		private void setHighlightedNode(string id) {
			nodeController.HighlightedNode = id == "" ? null : GraphController.Graph.GetNodeFromGameObjectName(id);
		}
		
		public void SetConnectionNode(ConnectionMode mode) => Synchronize("setConnectionNode", (int) mode);
		
		[RPC]
		private void setConnectionNode(int mode) {
			graphController.ConnectionMode.Value = (ConnectionMode) mode;
		}
		
		public void SetSelectedNode(string id) => Synchronize("setSelectedNode", id);
		
		[RPC]
		private void setSelectedNode(string id) {
			nodeController.SelectedNode = id == null ? null : GraphController.Graph.GetNodeFromGameObjectName(id);
		}
		
		private void Synchronize(string method, params object[] args) {
			NetworkView.RPC(method, RPC_MODE, args);
		}

		#endregion

		public bool IsServer() {
			return Environment == Environment.PC && Application.isEditor || Environment == Environment.Cave && Lzwp.sync.isMaster;
		}

		public bool IsClient() {
			return Environment == Environment.PC && !Application.isEditor || Environment == Environment.Cave && !Lzwp.sync.isMaster;
		}

		public void CloseClient() {
			if(Environment != Environment.PC || IsServer())
				return;
			Network.Disconnect();
		}
		
		private InputController inputController;
		private GraphController graphController;
		private NodeController nodeController;
		private ConnectionController connectionController;
		
		private void Awake() {
			inputController = GetComponent<InputController>();
			nodeController = GetComponent<NodeController>();
			graphController = GetComponent<GraphController>();
			connectionController = GetComponent<ConnectionController>();
			
			NetworkView = GetComponent<NetworkView>();
			if (Environment == Environment.PC) {
				if (Application.isEditor) {
					Network.InitializeServer(1, PORT);
				} else {
					Network.Connect("localhost", PORT);
				}
			}
			if (IsServer()) {
				nodeSyncBuffer = new NodeSyncBuffer(SyncLoadedNodes, SyncUnloadedNodes);
				nodeController.OnNodeLoaded += (node, position) => nodeSyncBuffer.OnNodeLoaded(node.ID, position);
				nodeController.OnNodeUnloaded += node => nodeSyncBuffer.OnNodeUnloaded(node.ID);
				nodeController.OnNodeLoadSessionEnded += nodeSyncBuffer.SyncRemaining;
				
				connectionSyncBuffer = new ConnectionSyncBuffer(
					SyncBufferFunction("syncConnection", true),
					SyncBufferFunction("syncConnection", false)
				);
				connectionController.OnConnectionLoadSessionEnded += connectionSyncBuffer.SyncRemaining;
			}
		}

		private Action<string> SyncBufferFunction(string rpcName, bool loaded) => stream => Synchronize(rpcName, stream, loaded);
	}
}
#pragma warning restore 618