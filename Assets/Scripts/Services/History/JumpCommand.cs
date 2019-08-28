using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Controllers;
using InputModule.Binding;

namespace Services.History
{
    public class JumpCommand : ICommand
    {
        private Model.Node _prevNode;
        private Model.Node _targetNode;
        public NodeController nodeController;

        public JumpCommand(Model.Node prevNode, Model.Node targetNode, ref NodeController controller)
        {
            _prevNode = prevNode;
            _targetNode = targetNode;
            nodeController = controller;
        }
        #region ICommand functions

        public void Execute() //do action forward
        {
            Debug.Log("Executed Redo!");
            nodeController.SelectedNode = _targetNode;
        }

        public void UnExecute() //do action backward
        {
            Debug.Log("Executed Undo!");
            if (_prevNode == null)
            {
                //TODO - leave node view to free flight
                return;
            }
            else
            {
                nodeController.SelectedNode = _prevNode;
            }
        }
        #endregion
    }

}
