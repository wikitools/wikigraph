using Controllers;
using Services.History;
using System;

namespace Services.History.Actions {
	public class ModeChangeAction<M> : UserAction {
		private M newMode;
		public static Action<M, bool> changeMode;
		bool isRouteAction;

		public ModeChangeAction(M mode, bool isRoute) {
			newMode = mode;
			isRouteAction = isRoute;
		}

		private void passChangingMode(M mode) {
			changeMode(mode, isRouteAction);
		}

		public void Execute() {
			passChangingMode(newMode);
		}

		public void UnExecute() {
			passChangingMode((M)(object)(Convert.ToInt32(newMode) == 0 ? 1 : 0));
		}

		public bool IsRoute() {
			return isRouteAction;
		}
	}
}