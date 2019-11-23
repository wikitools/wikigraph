using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Model;
using System.Linq;
using System;
using System.Threading;

namespace Controllers {
	public class NodeLoaderController : MonoBehaviour {
		private NodeController nodeController;
		private GraphController graphController;
		private ConnectionController connectionController;

		public int nodeStartingAmount = 1000;
		public int maxNodeLimit = 2000;
		private int singleRemoveAmount;

		public bool isReady = false;

		// lowPriorityNodes are nodes about to be deleted
		private List<uint> lowPriorityNodes = new List<uint>();
		// highPriorityNodes are nodes that cannot be deleted
		private List<uint> highPriorityNodes = new List<uint>();

		private void Awake() {
			nodeController = GetComponent<NodeController>();
			graphController = GetComponent<GraphController>();
			connectionController = GetComponent<ConnectionController>();
		}

		void Start() {
			singleRemoveAmount = connectionController.ConnectionDistribution.MaxVisibleNumber + 1;
			nodeController.OnNodeLoaded += (node, position) => AddHighPriorityNode(node.ID);
			nodeController.OnNodeLoaded += (node, position) => UnloadHandler();
			connectionController.OnConnectionRangeChanged += (start, end, count) => BehaviourConnectionChange();
			graphController.ConnectionMode.OnValueChanged += (mode) => BehaviourConnectionChange();
		}
		
		void Update() {
		}

		private void UnloadHandler() {
			if (GraphController.Graph.IdNodeMap.Count < maxNodeLimit) return;
			for(var n = 0; n < singleRemoveAmount; ++n) {
				if (lowPriorityNodes.Count == 0) {
					MoveNodePriorityToLow();
					if (lowPriorityNodes.Count == 0) return;
				}
				var toDelete = lowPriorityNodes.First();
				if (CheckIfInConnectionMap(toDelete)) {
					AddHighPriorityNode(toDelete);
					continue;
				}

				nodeController.NodeLoadManager.UnloadNode(toDelete);
				lowPriorityNodes.Remove(toDelete);
				//Debug.Log("Unloaded: " + toDelete.ToString());
			}

			nodeController.OnNodeLoadSessionEnded?.Invoke();
		}

		public void AddLowPriorityNode(uint id) {
			if (!highPriorityNodes.Contains(id) && !lowPriorityNodes.Contains(id)) {
				lowPriorityNodes.Add(id);
			}
		}

		public void AddHighPriorityNode(uint id) {
			if (isReady == true) {
				if (lowPriorityNodes.Contains(id)) {
					lowPriorityNodes.Remove(id);
				}
				if (!highPriorityNodes.Contains(id)) {
					highPriorityNodes.Add(id);
				}
			}
		}

		public void MoveNodePriorityToLow(uint id) {
			// First overload - moving node of given ID to low priority nodes
			if (highPriorityNodes.Contains(id))
				highPriorityNodes.Remove(id);
			AddLowPriorityNode(id);
		}

		public void MoveNodePriorityToLow() {
			// Second overload - we move oldest node from high priority to low priority
			if(highPriorityNodes.Count > 0)
				MoveNodePriorityToLow(highPriorityNodes.First());
		}

		public void NodeChangeStateBehaviour(Node node, NodeState state) {
			// Possible redundancy, although better structuralized if we desire to change behaviour
			if (state == NodeState.HIGHLIGHTED) {
				// Highlited node - save it
				BehaviourNodeInteracted(node);
			}
			if (node.State == NodeState.HIGHLIGHTED && state == NodeState.ACTIVE) {
				// quick hover - we keep all freshly loaded nodes
				BehaviourNodeInteracted(node);
			}
			else if (node.State == NodeState.HIGHLIGHTED && state == NodeState.SELECTED) {
				// selected node - we keep all freshly loaded nodes
				BehaviourNodeInteracted(node);
			}
		}

		public void BehaviourNodeInteracted(Node node) {
			// If user interacts with node, we keep this node and loaded neighbours
			var neighbours = connectionController.GetNodeNeighbours(node).ToList();
			for (var n = 0; n < Math.Min(neighbours.Count, connectionController.ConnectionDistribution.MaxVisibleNumber); ++n) {
				AddHighPriorityNode(neighbours[n]);
			}
			AddHighPriorityNode(node.ID);
		}

		private bool CheckIfInConnectionMap(uint node) {
			foreach (var com in GraphController.Graph.ConnectionObjectMap) {
				var connection = com.Key;
				var node1 = connection.Item1;
				var node2 = connection.Item2;
				if (node1.ID == node || node2.ID == node) return true;
			}
			return false;
		}
		
		private void BehaviourConnectionChange() {
			// Changed connections on selected node -> Iterate through connection map and add new nodes to highPriority ones
			foreach (var com in GraphController.Graph.ConnectionObjectMap) {
				var connection = com.Key;
				var node1 = connection.Item1;
				var node2 = connection.Item2;
				AddHighPriorityNode(node1.ID);
				AddHighPriorityNode(node2.ID);
			}
		}
	}
}