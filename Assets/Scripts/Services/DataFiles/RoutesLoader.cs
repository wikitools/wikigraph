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
		public static Func<uint, Node> getRouteNode;



		public RoutesLoader(string dataFilePostfix = "") {
			routesReader = new RoutesReader(dataFilePostfix);
		}

		public int routesNumber() {
			return routesReader.numberOfRoutes();
		}

		public string[] routesNames() {
			return routesReader.namesOfRoutes();
		}

		public Stack<UserAction> loadRoute(int index) {
			Stack<UserAction> userActions = new Stack<UserAction>();
			Node old = null;
			while (routesReader.isNotEOF(index)) {
				string[] line = routesReader.readLine(index).Split(separator);
				UserAction action = null;
				if (line[0] == USERACTION_TYPE.MODE_CHANGE.ToString()) {
					if (line[1] == CONNECT_MODE.CHILDREN.ToString()) {
						action = new ModeChangeAction<CONNECT_MODE>(CONNECT_MODE.CHILDREN);
					}
					else if (line[1] == CONNECT_MODE.PARENTS.ToString()) {
						action = new ModeChangeAction<CONNECT_MODE>(CONNECT_MODE.PARENTS);
					}
				}
				else if (line[0] == USERACTION_TYPE.NODE_SELECTED.ToString()) {
					Node newNode = getRouteNode(Convert.ToUInt32(line[1])); //possibility of bug
					action = new NodeSelectedAction(old, newNode);
					old = newNode;
				}
				
				userActions.Push(action);
			}
			return userActions;
		}

		public void Dispose() {
			routesReader.Dispose();
		}
	}

}
