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
				if(userAction.IsRoute()) onActionSetDirection(false);
				userAction.Execute();
				undoActionStack.Push(userAction);
				
			}
		}

		public void UndoAction() {
			if (undoActionStack.Count != 0) {
				UserAction userAction = undoActionStack.Pop();
				if(userAction.IsRoute()) onActionSetDirection(true);
				userAction.UnExecute();
				redoActionStack.Push(userAction);
				
			}
		}

		public void RegisterAction(UserAction userAction) {
			undoActionStack.Push(userAction);
			redoActionStack.Clear();
		}

		
		
	}
}
