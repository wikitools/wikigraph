using UnityEngine;

namespace Controllers {
	public class HistoryController : MonoBehaviour {

		private NodeController nodeController;
		private NetworkController networkController;
		public HistoryService historyService;

		bool nodeChangedByHistory = false;

		void Awake() {
			networkController = GetComponent<NetworkController>();
			if (networkController.IsServer()) {
				nodeController = GetComponent<NodeController>();
				historyService = new HistoryService();
				nodeController.OnSelectedNodeChanged += (oldNode, newNode) => {
					if (!nodeChangedByHistory) historyService.RegisterAction(new NodeSelectedAction(oldNode, newNode));
					nodeChangedByHistory = false;
				};
				NodeSelectedAction.selectNodeAction = node => {
					nodeChangedByHistory = true;
					nodeController.ForceSetSelect(node);
				};
			}
		}
	}
}
