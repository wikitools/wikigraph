using Controllers;
using InputModule.Binding;
using UnityEngine;

namespace InputModule.Processor {
	public abstract class InputProcessor {
		protected InputConfig Config;
		protected InputBinding Binding;
		protected InputController Controller;


		public InputProcessor(InputConfig config, InputBinding binding, InputController controller) {
			Config = config;
			Binding = binding;
			Controller = controller;

		}

		protected Transform EntityTransform => Controller.CameraController.Entity.transform;
		
		protected void ExitNodeTraverseMode() => Controller.NetworkController.SetGraphMode(GraphMode.FREE_FLIGHT);


		protected void RedoUserAction() { Controller.HistoryController.historyService.RedoAction(); }
		protected void UndoUserAction() { Controller.HistoryController.historyService.UndoAction(); }

		protected void OnNodeChosen(Ray ray) {
			RaycastHit raycastHit;
			if (RaycastNode(ray, out raycastHit)) {
				Controller.NetworkController.SetSelectedNode(raycastHit.collider.gameObject.name);
			}
		}

		protected void OnNodePointed(Ray ray) {
			RaycastHit raycastHit;
			Controller.NetworkController.SetHighlightedNode(RaycastNode(ray, out raycastHit) ? raycastHit.collider.gameObject.name : "");
		}

		private bool RaycastNode(Ray ray, out RaycastHit hit) => Physics.Raycast(ray, out hit, float.MaxValue, LayerMask.GetMask("Node"));


	}
}