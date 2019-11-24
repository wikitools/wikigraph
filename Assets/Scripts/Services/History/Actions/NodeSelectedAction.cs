using Model;
using System;

namespace Services.History.Actions {
	public class NodeSelectedAction : UserAction {
		private uint? oldSelectedNode;
		private uint? newSelectedNode;
		bool isRouteAction;
		public static Action<uint?, bool> selectNodeAction;

		public NodeSelectedAction(uint? oldNode, uint? newNode, bool isRoute) {
			oldSelectedNode = oldNode;
			newSelectedNode = newNode;
			isRouteAction = isRoute;
		}

		private void passSelectAction(uint? node) {
			selectNodeAction(node, isRouteAction);
		}

		#region UserAction functions
		public void Execute() {
			passSelectAction(newSelectedNode);
		}

		public void UnExecute() {
			passSelectAction(oldSelectedNode);
		}

		public bool IsRoute() {
			return isRouteAction;
		}
		#endregion
	}
}