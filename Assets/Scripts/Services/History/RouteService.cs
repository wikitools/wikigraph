using Controllers;
using Services.History.Actions;
using Services.Routes;
using Services.Search;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Services.History {

	public class RouteService : ActionService {
		bool playsRoute = false;
		public RouteLoader routesLoader;
		
		int secondsToNextRoute;
		private ActionController controller;

		public RouteService(ActionController controller, int seconds, string routesPath) {
			routesLoader = new RouteLoader(routesPath);
			secondsToNextRoute = seconds;
			
			this.controller = controller;
		}

		
		public void loadChosenRoute(int index) {
			redoActionStack = routesLoader.loadRoute(index);
			undoActionStack.Clear();
		}

		public void startRoute(int index) {
			loadChosenRoute(index);
			controller.networkController.SyncRoutePlaying(true);
		}

		public IEnumerator autoRoutes() {
			playsRoute = true;
			while (redoActionStack.Count != 0 && playsRoute) {
				RedoAction();
				if (redoActionStack.Count > 0) {
					yield return new UnityEngine.WaitForSeconds(secondsToNextRoute);
				}
			}
			playsRoute = false;
			controller.networkController.SyncRoutePlaying(false);
		}

		public void stopPlayingRoute() {
			playsRoute = false;
		}

		public bool isPlayingRoute() {
			return playsRoute;
		}

	}
}