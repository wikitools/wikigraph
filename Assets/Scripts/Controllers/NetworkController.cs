using System;
using Model;
using Services.SyncBuffer;
using UnityEngine;

#pragma warning disable 618
namespace Controllers {
	public class NetworkController : MonoBehaviour {
		private readonly RPCMode RPC_MODE = RPCMode.AllBuffered;

		private NetworkView NetworkView;
		private NodeSyncBuffer nodeSyncBuffer;

		private static readonly int PORT = 25000;

		private Environment Environment => inputController.Environment;

		#region RPC Sync

		public void SyncConnectionScrolled(int direction) => Synchronize("syncConnectionScrolled", direction);

		[RPC]
		private void syncConnectionScrolled(int direction) {
			connectionController.UpdateVisibleConnections(direction);
		}

		private void SyncLoadedNodes(string stream) => Synchronize("syncNodes", stream, true);

		private void SyncUnloadedNodes(string stream) => Synchronize("syncNodes", stream, false);

		[RPC]
		private void syncNodes(string stream, bool loaded) {
			if (loaded) //TODO add node unloading sync once we support it
				NodeSyncBuffer.ParseLoadedNodes(stream).ForEach(node => nodeController.NodeLoadManager.LoadNode(node.ID, node.Position));
		}

		public void SetGraphMode(GraphMode mode) => Synchronize("setGraphMode", (int) mode);

		[RPC]
		private void setGraphMode(int value) {
			graphController.GraphMode.Value = (GraphMode) value;
		}

		public void ToggleInfoSpace() => Synchronize("toggleInfoSpace");

		[RPC]
		private void toggleInfoSpace() {
			infoSpaceController.ToggleVisibility();
		}

		public void SetHighlightedNode(Node node) => SetHighlightedNode(NodeToID(node));

		public void SetHighlightedNode(string id) => Synchronize("setHighlightedNode", id);

		[RPC]
		private void setHighlightedNode(string id) {
			nodeController.HighlightedNode = IDToNode(id);
		}

		public void SetConnectionMode(ConnectionMode mode) => Synchronize("setConnectionMode", (int) mode);

		[RPC]
		private void setConnectionMode(int mode) {
			graphController.ConnectionMode.Value = (ConnectionMode) mode;
		}

		public void SetSelectedNode(Node node) => SetSelectedNode(NodeToID(node));

		public void SetSelectedNode(string id) => Synchronize("setSelectedNode", id);

		[RPC]
		private void setSelectedNode(string id) {
			nodeController.NodeStateManager.ForceSetSelectedNode(IDToNode(id));
		}

		private void Synchronize(string method, params object[] args) {
			NetworkView.RPC(method, RPC_MODE, args);
		}

		#endregion

		private string NodeToID(Node node) => node != null ? node.ID.ToString() : "";
		private Node IDToNode(string id) => id == "" ? null : GraphController.Graph.GetNodeFromGameObjectName(id);

		public bool IsServer() {
			return Environment == Environment.PC && Application.isEditor || Environment == Environment.Cave && Lzwp.sync.isMaster;
		}

		public bool IsClient() {
			return Environment == Environment.PC && !Application.isEditor || Environment == Environment.Cave && !Lzwp.sync.isMaster;
		}

		public void CloseClient() {
			if (Environment != Environment.PC || IsServer())
				return;
			Network.Disconnect();
		}

		private InputController inputController;
		private GraphController graphController;
		private NodeController nodeController;
		private ConnectionController connectionController;
		private InfoSpaceController infoSpaceController;

		private void Awake() {
			inputController = GetComponent<InputController>();
			nodeController = GetComponent<NodeController>();
			graphController = GetComponent<GraphController>();
			connectionController = GetComponent<ConnectionController>();
			infoSpaceController = (InfoSpaceController) Resources.FindObjectsOfTypeAll(typeof(InfoSpaceController))[0];

			NetworkView = GetComponent<NetworkView>();

			if(inputController.Environment == Environment.Cave)
				Lzwp.OnLZWPlibInitialize += Initialize;
			else
				Initialize();
		}

		private void Initialize() {
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
			}
		}
	}
}
#pragma warning restore 618