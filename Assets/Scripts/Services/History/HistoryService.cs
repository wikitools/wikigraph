using Controllers;
using Services.History.Actions;
using Services.Routes;
using Services.Search;
using System.Collections;
using System.Collections.Generic;

namespace Services.History {
	public class HistoryService {

		#region History
		private readonly Stack<UserAction> undoActionStack = new Stack<UserAction>();
		private readonly Stack<UserAction> redoActionStack = new Stack<UserAction>();

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

		#endregion
		#region Routes
		private Stack<UserAction> redoSavedRoute = new Stack<UserAction>();
		private Stack<UserAction> undoSavedRoute = new Stack<UserAction>();
		bool playsRoute = false;
		public RouteLoader routesLoader;
		public SearchLoader searchLoader;
		int secondsToNextRoute;
		private HistoryController controller;

		public HistoryService(HistoryController controller, int seconds, int number, string routesPath, string searchPath) {
			routesLoader = new RouteLoader(routesPath);
			secondsToNextRoute = seconds;
			searchLoader = new SearchLoader(number, searchPath);
			this.controller = controller;
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

		public void startRoute(int index) {
			loadChosenRoute(index);
			controller.NetworkController.SyncRoutePlaying(true);
		}

		public IEnumerator autoRoutes() {
			playsRoute = true;
			while (redoSavedRoute.Count != 0 && playsRoute) {
				RedoRoute();
				if (redoSavedRoute.Count > 0) {
					yield return new UnityEngine.WaitForSeconds(secondsToNextRoute);
				}
			}
			playsRoute = false;
			controller.NetworkController.SyncRoutePlaying(false);
		}

		public void stopPlayingRoute() {
			playsRoute = false;
		}

		public bool isPlayingRoute() {
			return playsRoute;
		}
		#endregion
	}
}
