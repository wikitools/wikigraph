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
		private Dictionary<Node, GameObject> nodes = new Dictionary<Node, GameObject>();
	
		public GameObject NodePrefab;
		public GameObject PoolNodeContainer;
		public int NodePreloadNumber;
		public float WorldRadius;
		
		public NodeController NodeController { get; private set; }
	
		void Start () {
			NodeController = GetComponent<NodeController>();
			
			nodeLoader = new NodeLoader();
			nodePool = new GameObjectPool(NodePrefab, NodePreloadNumber, PoolNodeContainer);
		
			for (uint i = 0; i < nodeLoader.GetNodeNumber(); i++) {
				LoadNode(i);
			}
		}

		private void LoadNode(uint id) {
			Node node = nodeLoader.LoadNode(id);
			GameObject nodeGO = nodePool.Spawn();
			NodeController.InitializeNode(node, ref nodeGO, Random.insideUnitSphere * WorldRadius);
			nodes[node] = nodeGO;
		}
	
		void Update () {
		
		}

		private void OnDestroy() {
			nodeLoader.Dispose();
		}
	}
}
