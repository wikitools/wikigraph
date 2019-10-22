using Controllers;
using Model;
using Services.History.Actions;
using System;
using System.Collections.Generic;



namespace Services.RoutesFiles {
	public class RoutesLoader : IDisposable {
		private enum USERACTION_TYPE {
			MODE_CHANGE,
			NODE_SELECTED
		};

		private enum CONNECT_MODE {
			CHILDREN,
			PARENTS
		};

		private RoutesReader routesReader;
		private static char separator = ';';
		

		public RoutesLoader(string dataFilePostfix = "") {
			routesReader = new RoutesReader(dataFilePostfix);
		}

		public int routesNumber() {
			return routesReader.numberOfRoutes();
		}

		public Stack<UserAction> loadRoute(int index) {
			Stack<UserAction> userActions = new Stack<UserAction>();
			Node old = null;
			while (routesReader.isNotEOF(index)) {
				string[] line = routesReader.readLine(index).Split(separator);
				UserAction action;
				if (line[0] == USERACTION_TYPE.MODE_CHANGE.ToString()) {
					if (line[1] == CONNECT_MODE.CHILDREN.ToString()) {
						action = new ModeChangeAction<CONNECT_MODE>(CONNECT_MODE.CHILDREN);
					}
					else if (line[1] == CONNECT_MODE.PARENTS.ToString()) {
						action = new ModeChangeAction<CONNECT_MODE>(CONNECT_MODE.PARENTS);
					}
				}
				else if (line[0] == USERACTION_TYPE.NODE_SELECTED.ToString()) {
					Node newNode = null;
					action = new NodeSelectedAction(old, newNode);
					old = newNode;
				}
			}
			return userActions;
		}

		public void Dispose() {
			routesReader.Dispose();
		}
	}

}
