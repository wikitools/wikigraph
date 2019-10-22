using System.Collections.Generic;
using Model;
using UnityEngine;

namespace Services {
	public class Graph {
		private readonly Logger<Graph> logger = new Logger<Graph>();
		public Dictionary<Node, GameObject> NodeObjectMap { get; } = new Dictionary<Node, GameObject>();
		public Dictionary<Model.Connection.Connection, GameObject> ConnectionObjectMap { get; } = new Dictionary<Model.Connection.Connection, GameObject>();
		public Dictionary<uint, Node> IdNodeMap { get; } = new Dictionary<uint, Node>();
		public Dictionary<Model.Connection.Connection, GameObject> ConnectionNodes = new Dictionary<Model.Connection.Connection, GameObject>();

		public Node GetNodeFromGameObjectName(string name) {
			uint id;
			if (!uint.TryParse(name, out id) || !IdNodeMap.ContainsKey(id)) {
				logger.Warning($"GameObject name {name} is not a node id");
				return null;
			}
			return IdNodeMap[id];
		}
	}
}