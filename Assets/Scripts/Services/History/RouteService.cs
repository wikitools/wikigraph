using Controllers;
using Services.History.Actions;
using Services.Routes;
using Services.Search;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Services.History {

	public class RouteService : ActionService {
		public RouteLoader routesLoader;
		
		int secondsToNextRoute;
		private ActionController controller;
		public bool IsRoutePlaying {
			get;
			set;
		}
		public RouteService(ActionController controller, int seconds, string routesPath) {
			IsRoutePlaying = false;
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
			IsRoutePlaying = true;
			while (redoActionStack.Count != 0 && IsRoutePlaying) {
				RedoAction();
				if (redoActionStack.Count > 0) {
					yield return new UnityEngine.WaitForSeconds(secondsToNextRoute);
				}
			}
			IsRoutePlaying = false;
			controller.networkController.SyncRoutePlaying(false);
		}


	}
}