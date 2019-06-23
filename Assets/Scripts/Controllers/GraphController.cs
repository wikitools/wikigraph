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
	
		public GameObject NodePrefab;
		public GameObject ConnectionPrefab;
		public GraphContainers Containers;

		public GameObject Infographic; //TODO: move
		
		public int NodePreloadNumber;
		public float WorldRadius;
		public int NodeLoadedLimit;
		public bool LoadTestNodeSet;
		
		public Graph Graph { get; } = new Graph();

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
				OnActiveNodeChanged(Graph.GetNodeFromObject(activeNode));
			}
		}
		public Action<Node?> OnActiveNodeChanged;

		public void LoadNode(uint id) {
			if(Graph.IdNodeMap.ContainsKey(id)) return;
			Node node = nodeLoader.LoadNode(id);
			Graph.IdNodeMap[id] = node;
			GameObject nodeGO = nodePool.Spawn();
			NodeController.InitializeNode(node, ref nodeGO, Random.insideUnitSphere * WorldRadius);
			Graph.NodeObjectMap[node] = nodeGO;
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
	public class GraphContainers {
		public GameObject NodeContainer;
		public GameObject ConnectionsContainer;
		public GameObject NodePoolContainer;
	}

	public enum GraphMode {
		FREE_FLIGHT, NODE_TRAVERSE
	}
}
