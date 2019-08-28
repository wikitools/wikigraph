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
        public NodeController nodeController { get; private set; }

        public JumpCommand(Model.Node prevNode, Model.Node targetNode, ref NodeController controller)
        {
            _prevNode = prevNode;
            _targetNode = targetNode;
            nodeController = controller;
            Execute();
        }
        #region ICommand functions

        public void Execute() //do action forward
        {
            nodeController.SelectedNode = _targetNode;
        }

        public void UnExecute() //do action backward
        {
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
