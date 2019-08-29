using System;
using Services;
using UnityEngine;

#pragma warning disable 618
namespace Controllers {
	public class NetworkController: MonoBehaviour {
		private readonly RPCMode RPC_MODE = RPCMode.AllBuffered;
		
		private NetworkView NetworkView;
		private NodeSyncParser NodeSyncParser;

		private static readonly int PORT = 25000;

		private Environment Environment => InputController.Environment;

		private void SyncLoadedNodes(string nodes) {
			Synchronize("syncNodes", nodes, true);
		}
		
		private void SyncUnloadedNodes(string nodes) {
			Synchronize("syncNodes", nodes, false);
		}
		
		[RPC]
		private void syncNodes(string nodes, bool loaded) {
			
		}
		
		public void SetGraphMode(GraphMode mode) {
			Synchronize("setGraphMode", (int) mode);
		}
		
		[RPC]
		private void setGraphMode(int value) {
			GraphController.GraphMode.Value = (GraphMode) value;
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

		public bool IsServer() {
			return Environment == Environment.PC && Application.isEditor || Environment == Environment.Cave && Lzwp.sync.isMaster;
		}

		public bool IsClient() {
			return Environment == Environment.PC && !Application.isEditor || Environment == Environment.Cave && !Lzwp.sync.isMaster;
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
				} else {
					Network.Connect("localhost", PORT);
				}
			}
		}

		private void Start() {
			NodeSyncParser = new NodeSyncParser(SyncLoadedNodes, SyncUnloadedNodes);
			NodeController.NodeLoaded += (node, position) => NodeSyncParser.OnNodeLoaded(node.ID, position);
			NodeController.NodeUnoaded += node => NodeSyncParser.OnNodeUnloaded(node.ID);
		}
	}
}
#pragma warning restore 618