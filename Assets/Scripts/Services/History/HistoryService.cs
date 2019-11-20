using Services.History.Actions;
using Services.RoutesFiles;
using Services.SearchFiles;
using System;
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
		public RoutesLoader routesLoader;
		public SearchLoader searchLoader;
		public static Action startRouteAutoAction;
		public static Action endRouteAutoAction;
		int secondsToNextRoute;
		int numberOfDisplayedRoutes;
		string pathWikiVersion;

		public HistoryService(int seconds, int number, string routesPath,string searchPath, string prefix = "") {
			routesLoader = new RoutesLoader(routesPath, prefix);
			secondsToNextRoute = seconds;
			numberOfDisplayedRoutes = number;
			pathWikiVersion = searchPath;
			searchLoader = new SearchLoader(number, searchPath);
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

		public string[] getNames() {
			return routesLoader.routesNames();
		}

		public int[] getLengths() {
			return routesLoader.routeLengths();
		}

		public void startRoute(int index) {
			loadChosenRoute(index);
			startRouteAutoAction();
		}

		public IEnumerator autoRoutes() {
			playsRoute = true;
			while (redoSavedRoute.Count != 0 && playsRoute) {
				RedoRoute();
				if (redoSavedRoute.Count > 0) yield return new UnityEngine.WaitForSeconds(secondsToNextRoute);
			}
			playsRoute = false;
			endRouteAutoAction();
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
