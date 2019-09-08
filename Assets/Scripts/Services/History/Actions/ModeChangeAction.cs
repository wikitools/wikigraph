using Controllers;
using Services.History;
using System;

namespace Services.History.Actions {
	public class ModeChangeAction<M> : UserAction {
		private M newMode;
		public static Action<M> changeMode;

		public ModeChangeAction(M mode) {
			newMode = mode;
		}

		private static void passChangingMode(M mode) {
			changeMode(mode);
		}

		public void Execute() {
			passChangingMode(newMode);
		}

		public void UnExecute() {
			passChangingMode((M)(object)(Convert.ToInt32(newMode) == 0 ? 1 : 0));
		}
	}
}