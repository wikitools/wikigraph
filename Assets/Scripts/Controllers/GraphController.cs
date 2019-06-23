using System;
using System.Collections.Generic;
using Model;
using Services;
using Services.DataFiles;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Controllers {
	public class GraphController : MonoBehaviour {
		private readonly Logger<GraphController> LOGGER = new Logger<GraphController>();
		
		private NodeLoader nodeLoader;
		private GameObjectPool nodePool;
		public Dictionary<Node, GameObject> NodeObjectMap { get; } = new Dictionary<Node, GameObject>();
		public Dictionary<uint, Node> IdNodeMap { get; } = new Dictionary<uint, Node>();
	
		public GameObject NodePrefab;
		public GameObject ConnectionPrefab;
		public Containers Containers;
		
		public int NodePreloadNumber;
		public float WorldRadius;
		public int NodeLoadedLimit;
		public bool LoadTestNodeSet;

		private GraphMode graphMode = GraphMode.FREE_FLIGHT;
		public GraphMode GraphMode {
			get { return graphMode; }
			set {
				graphMode = value;
				if (graphMode == GraphMode.FREE_FLIGHT)
					ActiveNode = null;
			}
		}

		public GameObject LastHighlightedNode { get; set; }
		
		private GameObject activeNode;
		public GameObject ActiveNode {
			get { return activeNode; }
			set {
				if(activeNode == value) return;
				activeNode = value;
				GraphMode = activeNode != null ? GraphMode.NODE_TRAVERSE : GraphMode.FREE_FLIGHT;
				OnActiveNodeChanged(GetNodeFromObject(activeNode));
			}
		}
		public Action<Node?> OnActiveNodeChanged;

		public Node? GetNodeFromObject(GameObject gameObject) {
			if (gameObject == null) return null;
			var name = gameObject.name;
			uint id;
			if (!uint.TryParse(name, out id) || !IdNodeMap.ContainsKey(id)) {
				LOGGER.Warning($"GameObject name {name} is not a node id");
				return null;
			}
			return IdNodeMap[id];
		}

		public GameObject GetObjectFromId(uint id) {
			if (!IdNodeMap.ContainsKey(id)) {
				LOGGER.Warning($"Id {id} is not a node id or is not loaded");
				return null;
			}
			var node = IdNodeMap[id];
			if (!NodeObjectMap.ContainsKey(node)) {
				LOGGER.Warning("Node is not loaded");
				return null;
			}
			return NodeObjectMap[node];
		}

		public void LoadNode(uint id) {
			if(IdNodeMap.ContainsKey(id)) return;
			Node node = nodeLoader.LoadNode(id);
			IdNodeMap[id] = node;
			GameObject nodeGO = nodePool.Spawn();
			NodeController.InitializeNode(node, ref nodeGO, Random.insideUnitSphere * WorldRadius);
			NodeObjectMap[node] = nodeGO;
		}
		
		public NodeController NodeController { get; private set; }

		void Awake() {
			NodeController = GetComponent<NodeController>();
		}
	
		void Start () {
			nodeLoader = new NodeLoader(LoadTestNodeSet ? "-test" : "");
			nodePool = new GameObjectPool(NodePrefab, NodePreloadNumber, Containers.NodePoolContainer);
			
			for (uint i = 0; i < Math.Min(NodeLoadedLimit, nodeLoader.GetNodeNumber()); i++) {
				LoadNode(i);
			}
		}
	
		void Update () {
		
		}

		private void OnDestroy() {
			nodeLoader.Dispose();
		}
	}

	[Serializable]
	public class Containers {
		public GameObject NodeContainer;
		public GameObject ConnectionsContainer;
		public GameObject NodePoolContainer;
	}

	public enum GraphMode {
		FREE_FLIGHT, NODE_TRAVERSE
	}
}
