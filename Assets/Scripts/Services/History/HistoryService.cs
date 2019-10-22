using Services.History.Actions;
using Services.RoutesFiles;
using System.Collections.Generic;

namespace Services.History {
	public class HistoryService {
		private readonly Stack<UserAction> undoActionStack = new Stack<UserAction>();
		private readonly Stack<UserAction> redoActionStack = new Stack<UserAction>();

		private Stack<UserAction> redoSavedRoute = new Stack<UserAction>();
		private Stack<UserAction> undoSavedRoute = new Stack<UserAction>();

		private RoutesLoader routesLoader;


		public void RedoAction() {
			if (redoActionStack.Count != 0) {
				UserAction userAction = redoActionStack.Pop();
				userAction.Execute();
				undoActionStack.Push(userAction);
			}
		}

		public void UndoAction() {
			if (undoActionStack.Count != 0) {
				UserAction userAction = undoActionStack.Pop();
				userAction.UnExecute();
				redoActionStack.Push(userAction);
			}
		}

		public void RegisterAction(UserAction userAction) {
			undoActionStack.Push(userAction);
			redoActionStack.Clear();
		}

		public void RedoRoute() {
			if (redoSavedRoute.Count != 0) {
				UserAction userAction = redoSavedRoute.Pop();
				userAction.Execute();
				undoSavedRoute.Push(userAction);
			}
		}

		public void UndoRoute() {
			if (undoSavedRoute.Count != 0) {
				UserAction userAction = undoSavedRoute.Pop();
				userAction.UnExecute();
				redoSavedRoute.Push(userAction);
			}
		}

		public void loadChosenRoute(int index) {
			redoSavedRoute = routesLoader.loadRoute(index);
			undoSavedRoute.Clear();
		}
	}
}
