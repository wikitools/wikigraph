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

		

		private RoutesReader routesReader;
		private static char separator = ';';
		public static Func<uint, Node> getRouteNode;



		public RoutesLoader(string path, string dataFilePostfix = "") {
			routesReader = new RoutesReader(path, dataFilePostfix);
		}

		public int routesNumber() {
			return routesReader.numberOfRoutes();
		}

		public string[] routesNames() {
			return routesReader.namesOfRoutes();
		}

		public int[] routeLengths() {
			return routesReader.lengthOfRoutes();
		}

		public Stack<UserAction> loadRoute(int index) {
			Stack<UserAction> userActions = new Stack<UserAction>();
			Node old = null;
			while (routesReader.isNotEOF(index)) {
				string[] line = routesReader.readLine(index).Split(separator);
				UserAction action = null;
				if (line[0] == USERACTION_TYPE.MODE_CHANGE.ToString("d")) {
					if (line[1] == ConnectionMode.CHILDREN.ToString("d")) {
						action = new ModeChangeAction<ConnectionMode>(ConnectionMode.CHILDREN);
					}
					else if (line[1] == ConnectionMode.PARENTS.ToString("d")) {
						action = new ModeChangeAction<ConnectionMode>(ConnectionMode.PARENTS);
					}
				}
				else if (line[0] == USERACTION_TYPE.NODE_SELECTED.ToString("d")) {
					Node newNode = getRouteNode(Convert.ToUInt32(line[1]));
					action = new NodeSelectedAction(old, newNode);
					old = newNode;
				}
				
				userActions.Push(action);
			}

			Stack<UserAction> rev = new Stack<UserAction>();
			while(userActions.Count!=0) {
				rev.Push(userActions.Pop());
			}

			return rev;
		}

		public void Dispose() {
			routesReader.Dispose();
		}
	}

}
