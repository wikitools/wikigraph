using UnityEngine;

namespace Controllers {
	public class HistoryController : MonoBehaviour {

		private NodeController nodeController;
		private NetworkController networkController;
		private GraphController graphController;
		public HistoryService historyService;

		bool nodeChangedByHistory = false;
		bool modeChangedByHistory = false;

		void Awake() {
			networkController = GetComponent<NetworkController>();
			nodeController = GetComponent<NodeController>();
			graphController = GetComponent<GraphController>();
		}

		private void Start() {
			if (networkController.IsServer()) {
				historyService = new HistoryService();
				nodeController.OnSelectedNodeChanged += (oldNode, newNode) => {
					if (!nodeChangedByHistory) historyService.RegisterAction(new NodeSelectedAction(oldNode, newNode));
					nodeChangedByHistory = false;
				};
				NodeSelectedAction.selectNodeAction = node => {
					nodeChangedByHistory = true;
					nodeController.ForceSetSelect(node);
				};
				graphController.ConnectionMode.OnValueChanged += mode => {
					if (!modeChangedByHistory) historyService.RegisterAction(new ModeAction());
					modeChangedByHistory = false;
				};
				ModeAction.changeModeAction = delegate() {
					modeChangedByHistory = true;
					graphController.SwitchConnectionMode();
				};
			}
		}
	}
}
