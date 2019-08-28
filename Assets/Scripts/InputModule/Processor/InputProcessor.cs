using Controllers;
using InputModule.Binding;
using UnityEngine;

namespace InputModule.Processor {
	public abstract class InputProcessor {
		protected InputConfig Config;
		protected InputBinding Binding;
		protected InputController Controller;
        protected HistoryController History;

		public InputProcessor(InputConfig config, InputBinding binding, InputController controller, HistoryController history) {
			Config = config;
			Binding = binding;
			Controller = controller;
            History = history;

		}

		protected Transform EntityTransform => Controller.CameraController.Entity.transform;
		
		protected void ExitNodeTraverseMode() => Controller.GraphController.GraphMode.Value = GraphMode.FREE_FLIGHT;

        protected void Redo() { Debug.Log("Button pressed Redo!"); History.Redo(1); }
        protected void Undo() { Debug.Log("Button pressed Undo!"); History.Undo(1); }

        protected void OnNodeChosen(Ray ray) {
			RaycastHit raycastHit;
            if (RaycastNode(ray, out raycastHit)) {
                History.InsertInUnDoRedoForJump(Controller.NodeController.SelectedNode, GraphController.Graph.GetNodeFromObject(raycastHit.collider.gameObject));
			}
		}

		protected void OnNodePointed(Ray ray) {
			RaycastHit raycastHit;
			Controller.NodeController.HighlightedNode = RaycastNode(ray, out raycastHit) ? GraphController.Graph.GetNodeFromObject(raycastHit.collider.gameObject) : null;
		}
		
		private bool RaycastNode(Ray ray, out RaycastHit hit) => Physics.Raycast(ray, out hit, float.MaxValue, LayerMask.GetMask("Node"));


	}
}