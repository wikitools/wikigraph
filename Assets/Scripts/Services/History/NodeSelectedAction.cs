using Model;
using Services.History;
using System;
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
		if (oldSelectedNode == null) {
			return;
		}
		else {
			passSelectAction(oldSelectedNode);
		}
	}
	#endregion
}