using Controllers;
using Services.History;
using System;

public class NodeTraverseModeAction : UserAction {
	private GraphMode newGraphMode;
	public static Action<GraphMode> changeNodeTraverseMode;

	public NodeTraverseModeAction(GraphMode mode) {
		newGraphMode = mode;
	}

	private static void passChangingNodeTraverseMode(GraphMode mode) {
		changeNodeTraverseMode(mode);
	}

	public void Execute() {
		passChangingNodeTraverseMode(newGraphMode);
	}

	public void UnExecute() {
		passChangingNodeTraverseMode(newGraphMode == GraphMode.FREE_FLIGHT ? GraphMode.NODE_TRAVERSE : GraphMode.FREE_FLIGHT);
	}
}
