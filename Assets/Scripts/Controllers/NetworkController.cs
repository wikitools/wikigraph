using Services;
using UnityEngine;

#pragma warning disable 618
namespace Controllers {
	public class NetworkController : MonoBehaviour {
		private readonly RPCMode RPC_MODE = RPCMode.AllBuffered;

		private NetworkView NetworkView;
		private NodeSyncParser NodeSyncParser;

		private static readonly int PORT = 25000;

		private Environment Environment => InputController.Environment;

		#region RPC Sync

		private void SyncLoadedNodes(string nodeStream) {
			Synchronize("syncNodes", nodeStream, true);
		}

		private void SyncUnloadedNodes(string nodeStream) {
			Synchronize("syncNodes", nodeStream, false);
		}

		[RPC]
		private void syncNodes(string nodeStream, bool loaded) {
			if (loaded) //TODO add node unloading sync once we support it
				NodeSyncParser.ParseLoadedNodes(nodeStream).ForEach(node => NodeController.LoadNode(node.ID, node.Position));
		}

		public void SetGraphMode(GraphMode mode) {
			Synchronize("setGraphMode", (int)mode);
		}

		[RPC]
		private void setGraphMode(int value) {
			GraphController.GraphMode.Value = (GraphMode)value;
		}

		public void SetConnectionMode(ConnectionMode mode) {
			Synchronize("setConnectionMode", (int)mode);
		}

		[RPC]
		private void setConnectionMode(int value) {
			GraphController.ConnectionMode.Value = (ConnectionMode)value;
		}

		public void SetHighlightedNode(string id) {
			Synchronize("setHighlightedNode", id);
		}

		[RPC]
		private void setHighlightedNode(string id) {
			NodeController.HighlightedNode = id == "" ? null : GraphController.Graph.GetNodeFromGameObjectName(id);
		}

		public void SetSelectedNode(string id) {
			Synchronize("setSelectedNode", id);
		}

		[RPC]
		private void setSelectedNode(string id) {
			NodeController.SelectedNode = id == null ? null : GraphController.Graph.GetNodeFromGameObjectName(id);
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
			if (Environment != Environment.PC || IsServer())
				return;
			Network.Disconnect();
		}

		public InputController InputController { get; private set; }
		public GraphController GraphController { get; private set; }
		public NodeController NodeController { get; private set; }

		private void Awake() {
			InputController = GetComponent<InputController>();
			NodeController = GetComponent<NodeController>();
			GraphController = GetComponent<GraphController>();

			NetworkView = GetComponent<NetworkView>();
			if (Environment == Environment.PC) {
				if (Application.isEditor) {
					Network.InitializeServer(1, PORT);
				}
				else {
					Network.Connect("localhost", PORT);
				}
			}
			NodeSyncParser = new NodeSyncParser(SyncLoadedNodes, SyncUnloadedNodes);
			if (IsServer()) {
				NodeController.NodeLoaded += (node, position) => NodeSyncParser.OnNodeLoaded(node.ID, position);
				NodeController.NodeUnoaded += node => NodeSyncParser.OnNodeUnloaded(node.ID);
				NodeController.NodeLoadSessionEnded += NodeSyncParser.SyncRemainingNodes;
			}
		}
	}
}
#pragma warning restore 618