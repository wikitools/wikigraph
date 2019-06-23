using System;
using System.Collections.Generic;
using Model;
using Services;
using Services.DataFiles;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Controllers {
	public class GraphController : MonoBehaviour {
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

		public GraphMode GraphMode { get; set; } = GraphMode.FREE_FLIGHT;
		//public Action<GraphMode> OnModeChanged;
		
		public GameObject LastHighlightedNode { get; set; }
		
		private GameObject activeNode;
		public GameObject ActiveNode {
			get { return activeNode; }
			set {
				activeNode = value;
				GraphMode = activeNode != null ? GraphMode.NODE_TRAVERSE : GraphMode.FREE_FLIGHT;
			}
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

		private void LoadNode(uint id) {
			Node node = nodeLoader.LoadNode(id);
			IdNodeMap[id] = node;
			GameObject nodeGO = nodePool.Spawn();
			NodeController.InitializeNode(node, ref nodeGO, Random.insideUnitSphere * WorldRadius);
			NodeObjectMap[node] = nodeGO;
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
