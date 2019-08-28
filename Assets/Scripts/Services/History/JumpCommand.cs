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
          // if(_prevNode == null) Debug.Log("Created history: null -> "+ _targetNode.ID+ " | "+ _targetNode.Title);
           // else Debug.Log("Created history: " + _prevNode.ID + " | " + _prevNode.Title + " -> " + _targetNode.ID + " | " + _targetNode.Title);
        }
        #region ICommand functions

        public void Execute() //do action forward
        {
            nodeController.SetNodeState(_targetNode, Model.NodeState.ACTIVE);
            nodeController.SelectedNode = _targetNode;
           // Debug.Log("Redo to " + nodeController.SelectedNode.ID + " | " + nodeController.SelectedNode.Title);
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
                nodeController.SetNodeState(_prevNode, Model.NodeState.ACTIVE);
                nodeController.SelectedNode = _prevNode;
            }
          //  Debug.Log("Undo to " + nodeController.SelectedNode.ID + " | " + nodeController.SelectedNode.Title);
        }
        #endregion
    }

}
