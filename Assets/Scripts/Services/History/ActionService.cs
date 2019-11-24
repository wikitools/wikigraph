using Controllers;
using Services.History.Actions;
using Services.Routes;
using Services.Search;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Services.History {
	public class ActionService {
		public Stack<UserAction> undoActionStack = new Stack<UserAction>();
		public Stack<UserAction> redoActionStack = new Stack<UserAction>();

		public Action<bool> onActionSetDirection;

		public void RedoAction() {
			if (redoActionStack.Count != 0) {
				UserAction userAction = redoActionStack.Pop();
				userAction.Execute();
				undoActionStack.Push(userAction);
				onActionSetDirection(false);
			}
		}

		public void UndoAction() {
			if (undoActionStack.Count != 0) {
				UserAction userAction = undoActionStack.Pop();
				userAction.UnExecute();
				redoActionStack.Push(userAction);
				onActionSetDirection(true);
			}
		}

		public void RegisterAction(UserAction userAction) {
			undoActionStack.Push(userAction);
			redoActionStack.Clear();
		}

		
		
	}
}
