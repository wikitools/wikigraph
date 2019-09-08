using Services.History;
using Services.History.Actions;
using UnityEngine;

namespace Controllers {
	public class HistoryController : MonoBehaviour {
		private NodeController nodeController;
		private NetworkController networkController;
		private GraphController graphController;
		public HistoryService HistoryService { get; private set; }

		bool nodeChangedByHistory = false;
		bool modeChangedByHistory = false;
		bool graphChangedByHistory = false;

		void Awake() {
			networkController = GetComponent<NetworkController>();
			nodeController = GetComponent<NodeController>();
			graphController = GetComponent<GraphController>();
		}

		private void Start() {
			if (networkController.IsServer()) {
				HistoryService = new HistoryService();
				nodeController.OnSelectedNodeChanged += (oldNode, newNode) => {
					if (!nodeChangedByHistory) HistoryService.RegisterAction(new NodeSelectedAction(oldNode, newNode));
					nodeChangedByHistory = false;
				};
				NodeSelectedAction.selectNodeAction = node => {
					nodeChangedByHistory = true;
					nodeController.ForceSetSelect(node);
				};
				graphController.ConnectionMode.OnValueChanged += mode => {
					if (!modeChangedByHistory) HistoryService.RegisterAction(new ModeChangeAction<ConnectionMode>(mode));
					modeChangedByHistory = false;
				};
				ModeChangeAction<ConnectionMode>.changeMode = mode => {
					modeChangedByHistory = true;
					graphController.SetConnectionMode(mode);
				};
			}
		}
	}
}