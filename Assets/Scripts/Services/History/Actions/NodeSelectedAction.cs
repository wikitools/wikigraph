using Model;
using System;

namespace Services.History.Actions {
	public class NodeSelectedAction : UserAction {
		private Node oldSelectedNode;
		private Node newSelectedNode;
		bool isRouteAction;
		public static Action<Node, bool> selectNodeAction;

		public NodeSelectedAction(Node oldNode, Node newNode, bool isRoute) {
			oldSelectedNode = oldNode;
			newSelectedNode = newNode;
			isRouteAction = isRoute;
		}

		private void passSelectAction(Node node) {
			selectNodeAction(node, isRouteAction);
		}


		#region UserAction functions

		//do action forward
		public void Execute() {
			passSelectAction(newSelectedNode);
		}

		//do action backward
		public void UnExecute() {
			passSelectAction(oldSelectedNode);
		}
		#endregion
	}
}