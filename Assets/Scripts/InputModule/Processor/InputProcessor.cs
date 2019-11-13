using Controllers;
using InputModule.Binding;
using Services;
using UnityEngine;

namespace InputModule.Processor {
	public abstract class InputProcessor {
		protected InputConfig Config;
		protected InputController Controller;

		public InputProcessor(InputConfig config, InputController controller) {
			Config = config;
			Controller = controller;
		}

		protected Transform EntityTransform => Controller.CameraController.Entity.transform;

		protected void ExitNodeTraverseMode() => Controller.NetworkController.SetGraphMode(GraphMode.FREE_FLIGHT);

		protected void BindHistoryEvents(int direction) {
			if(direction == 1)
				Controller.HistoryController.HistoryService.UndoAction();
			else if (direction == -1)
				Controller.HistoryController.HistoryService.RedoAction();
		}
		
		protected void ToggleInfoSpace() => Controller.InfoSpaceController.ToggleVisibility();
		protected void ToggleOperatorConsole() => Controller.ConsoleController.ToggleVisibility();

		protected void OnNodeChosen(Ray ray) {
			RaycastHit raycastHit;
			if (RaycastNode(ray, out raycastHit)) {
				Controller.NetworkController.SetSelectedNode(raycastHit.collider.gameObject.name);
			}
		}

		protected void OnNodePointed(Ray ray) {
			RaycastHit raycastHit;
			var id = RaycastNode(ray, out raycastHit) ? raycastHit.collider.gameObject.name : "";
			if (Controller.NodeController.IsNodeInteractable(id != "" ? raycastHit.collider.gameObject.layer : -1, id))
				Controller.NetworkController.SetHighlightedNode(id);
		}

		private bool RaycastNode(Ray ray, out RaycastHit hit) => Physics.Raycast(ray, out hit, float.MaxValue, LayerMask.GetMask("Node", "Connection Node"));
	}
}