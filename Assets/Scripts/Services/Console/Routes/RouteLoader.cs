using Controllers;
using Model;
using Services.History.Actions;
using System;
using System.Collections.Generic;



namespace Services.Routes {
	public class RouteLoader : IDisposable {
		private enum USER_ACTION_TYPE {
			MODE_CHANGED,
			NODE_SELECTED
		};

		public RouteReader routeReader;
		private static char SEPARATOR = ';';

		public RouteLoader(string path) {
			routeReader = new RouteReader(path);
		}

		public Stack<UserAction> loadRoute(int index) {
			Stack<UserAction> userActions = new Stack<UserAction>();
			uint? old = null;
			while (routeReader.isNotEOF(index)) {
				string[] line = routeReader.readLine(index).Split(SEPARATOR);
				UserAction action = null;
				if (line[0] == USER_ACTION_TYPE.MODE_CHANGED.ToString("d")) {
					if (line[1] == ConnectionMode.CHILDREN.ToString("d")) {
						action = new ModeChangeAction<ConnectionMode>(ConnectionMode.CHILDREN, true);
					}
					else if (line[1] == ConnectionMode.PARENTS.ToString("d")) {
						action = new ModeChangeAction<ConnectionMode>(ConnectionMode.PARENTS, true);
					}
				}
				else if (line[0] == USER_ACTION_TYPE.NODE_SELECTED.ToString("d")) {
					uint? newNode = Convert.ToUInt32(line[1]);
					action = new NodeSelectedAction(old, newNode, true);
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
			routeReader.Dispose();
		}
	}

}
