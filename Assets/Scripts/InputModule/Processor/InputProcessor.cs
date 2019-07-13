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
		
		protected void ExitNodeTraverseMode() => Controller.GraphController.GraphMode = GraphMode.FREE_FLIGHT;

		protected void OnNodeChosen(Ray ray) {
			RaycastHit raycastHit;
			if (RaycastNode(ray, out raycastHit)) {
				Controller.GraphController.ActiveNode = raycastHit.collider.gameObject;
			}
		}

		protected void OnNodePointed(Ray ray) {
			RaycastHit raycastHit;
			if (RaycastNode(ray, out raycastHit)) {
				Controller.NodeController.UpdateNodeHighlight(raycastHit.collider.gameObject);
			} else {
				Controller.NodeController.UpdateNodeHighlight(null);
			}
		}
		
		private bool RaycastNode(Ray ray, out RaycastHit hit) => Physics.Raycast(ray, out hit, float.MaxValue, LayerMask.GetMask("Node"));
	}
}