using Controllers;
using Services.History;
using System;

public class ModeAction : UserAction {
	public static Action<ConnectionMode> changeModeAction;
	private ConnectionMode connectionMode;

	public ModeAction(ConnectionMode mode) {
		connectionMode = mode;
	}

	private static void passChangeMode(ConnectionMode mode) {
		changeModeAction(mode);
	}

	public void Execute() {
		passChangeMode(connectionMode);
	}

	public void UnExecute() {
		passChangeMode(connectionMode == ConnectionMode.PARENTS ? ConnectionMode.CHILDREN : ConnectionMode.PARENTS);
	}
}