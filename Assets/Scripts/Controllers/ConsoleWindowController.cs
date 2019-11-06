using UnityEngine;

namespace Controllers {
	public class ConsoleWindowController : MonoBehaviour {

		public GameObject ConsoleWindowCanvas;
		
		private NetworkController networkController;
		private InputController inputController;
		private NodeController nodeController;
		private ConnectionController connectionController;

		// Use this for initialization
		void Awake() {
			networkController = GetComponent<NetworkController>();
			inputController = GetComponent<InputController>();
			nodeController = GetComponent<NodeController>();
			connectionController = GetComponent<ConnectionController>();
		}

		public void ToggleVisibility() {
			if(!networkController.IsServer())
				return;
			ConsoleWindowCanvas.SetActive(!ConsoleWindowCanvas.activeSelf);
			inputController.SetBlockInput(ConsoleWindowCanvas.activeSelf);
		}
	}
}