using System.Collections.Generic;
using Model;
using UnityEngine;

namespace Services {
	public class Graph {
		private readonly Logger<Graph> logger = new Logger<Graph>();
		public Dictionary<Node, GameObject> NodeObjectMap { get; } = new Dictionary<Node, GameObject>();
		public Dictionary<Model.Connection, GameObject> ConnectionObjectMap { get; } = new Dictionary<Model.Connection, GameObject>();
		public Dictionary<uint, Node> IdNodeMap { get; } = new Dictionary<uint, Node>();

		public Node GetNodeFromObject(GameObject gameObject) {
			if (gameObject == null) return null;
			return GetNodeFromGameObjectName(gameObject.name);
		}

		public Node GetNodeFromGameObjectName(string name) {
			uint id;
			if (!uint.TryParse(name, out id) || !IdNodeMap.ContainsKey(id)) {
				logger.Warning($"GameObject name {name} is not a node id");
				return null;
			}
			return IdNodeMap[id];
		}

		public GameObject GetObjectFromId(uint id) {
			if (!IdNodeMap.ContainsKey(id)) {
				logger.Warning($"Id {id} is not a node id or is not loaded");
				return null;
			}
			var node = IdNodeMap[id];
			if (!NodeObjectMap.ContainsKey(node)) {
				logger.Warning("Node is not loaded");
				return null;
			}
			return NodeObjectMap[node];
		}
	}
}