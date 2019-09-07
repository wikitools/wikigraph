using UnityEngine;

namespace Controllers {
	public class HistoryController : MonoBehaviour {

		private NodeController nodeController;
		public HistoryService historyService;

		bool nodeChangedByHistory = false;

		void Awake() {
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
