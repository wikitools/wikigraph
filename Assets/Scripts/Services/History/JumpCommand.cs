using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Services.History
{
    public class JumpCommand : ICommand
    {
        private Model.Node _prevNode;
        private Model.Node _targetNode;
        public JumpCommand(Model.Node prevNode, Model.Node targetNode)
        {
            _prevNode = prevNode;
            _targetNode = targetNode;
        }

        public void Execute()
        {
            
        }

        public void UnExecute()
        {
            throw new System.NotImplementedException();
        }
    }

}
