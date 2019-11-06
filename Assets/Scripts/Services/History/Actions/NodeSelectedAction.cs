using Model;
using System;

namespace Services.History.Actions {
	public class NodeSelectedAction : UserAction {
		private Node oldSelectedNode;
		private Node newSelectedNode;
		public static Action<Node> selectNodeAction;

		public NodeSelectedAction(Node oldNode, Node newNode) {
			oldSelectedNode = oldNode;
			newSelectedNode = newNode;
		}

		private void passSelectAction(Node node) {
			selectNodeAction(node);
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