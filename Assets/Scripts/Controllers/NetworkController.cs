using UnityEngine;

#pragma warning disable 618
namespace Controllers {
	public class NetworkController: MonoBehaviour {
		private NetworkView NetworkView;
		private RPCMode RPCMode = RPCMode.AllBuffered;

		private static readonly int PORT = 25000;

		private Environment Environment => InputController.Environment;

		public void SetGraphMode(GraphMode mode) {
			NetworkView.RPC("setGraphMode", RPCMode, (int) mode);
		}
		
		[RPC]
		private void setGraphMode(int value) {
			GraphController.GraphMode.Value = (GraphMode) value;
		}
		
		public void SetHighlightedNode(string id) {
			NetworkView.RPC("setHighlightedNode", RPCMode, id);
		}
		
		[RPC]
		private void setHighlightedNode(string id) {
			NodeController.HighlightedNode = id == "" ? null : GraphController.Graph.GetNodeFromGameObjectName(id);
		}
		
		public void SetSelectedNode(string id) {
			NetworkView.RPC("setSelectedNode", RPCMode, id);
		}
		
		[RPC]
		private void setSelectedNode(string id) {
			NodeController.SelectedNode = id == null ? null : GraphController.Graph.GetNodeFromGameObjectName(id);
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
	}
}
#pragma warning restore 618