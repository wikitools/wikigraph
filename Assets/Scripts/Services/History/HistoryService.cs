using Model;
using Services.History.Actions;
using Services.RoutesFiles;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Services.History {
	public class HistoryService {
		private readonly Stack<UserAction> undoActionStack = new Stack<UserAction>();
		private readonly Stack<UserAction> redoActionStack = new Stack<UserAction>();
		public static Action startRouteAutoAction;
		

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

		private Stack<UserAction> redoSavedRoute = new Stack<UserAction>();
		private Stack<UserAction> undoSavedRoute = new Stack<UserAction>();
		public bool playsRoute = false;
		public RoutesLoader routesLoader;


		public HistoryService(string prefix = "") {
			routesLoader = new RoutesLoader();
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

		public void startRoute(int index) {
			loadChosenRoute(index);
			startRouteAutoAction();
		}

		public IEnumerator autoRoutes() {
			playsRoute = true;
			while(redoSavedRoute.Count != 0 && playsRoute) {
				RedoRoute();
				yield return new UnityEngine.WaitForSeconds(7f);
			}
			playsRoute = false;
			
		}

		public void stopPlayingRoute() {
			playsRoute = false;
		}


	}
}
