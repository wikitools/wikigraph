using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Controllers;
using Services.History;

namespace Controllers
{
    public class HistoryController: MonoBehaviour
    {
        private Stack<ICommand> _Undocommands = new Stack<ICommand>();
        private Stack<ICommand> _Redocommands = new Stack<ICommand>();
        private NodeController nodeController;

        void Awake()
        {
            nodeController = GetComponent<NodeController>();
        }

        public void Redo(int levels)
        {
            for (int i = 1; i <= levels; i++)
            {
                if (_Redocommands.Count != 0)
                {
                    ICommand command = _Redocommands.Pop();
                    command.Execute();
                    _Undocommands.Push(command);
                }

            }
        }

        public void Undo(int levels)
        {
            for (int i = 1; i <= levels; i++)
            {
                if (_Undocommands.Count != 0)
                {
                    ICommand command = _Undocommands.Pop();
                    command.UnExecute();
                    _Redocommands.Push(command);
                }

            }
        }

        #region UndoHelperFunctions

        public void InsertInUnDoRedoForJump(Model.Node prevNode, Model.Node targetNode)
        {
            ICommand cmd = new JumpCommand(prevNode, targetNode, ref nodeController);
            _Undocommands.Push(cmd); _Redocommands.Clear();
        }

        #endregion
    }
}
