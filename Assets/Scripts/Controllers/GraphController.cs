using System.Collections.Generic;
using Model;
using Services;
using UnityEngine;

public class GraphController : MonoBehaviour {
	private NodeLoader nodeLoader;
	private GameObjectPool nodePool;
	private Dictionary<Node, GameObject> nodes = new Dictionary<Node, GameObject>();
	
	public GameObject NodePrefab;
	public GameObject PoolNodeContainer;
	public int NodePreloadNumber;
	public float WorldRadius;
	
	void Start () {
		nodeLoader = new NodeLoader();
		nodePool = new GameObjectPool(NodePrefab, NodePreloadNumber, PoolNodeContainer);
		
		for (uint i = 0; i < nodeLoader.GetNodeNumber(); i++) {
			GameObject nodeGO = nodePool.Spawn();
			nodeGO.transform.SetParent(transform, false);
			nodeGO.transform.position = Random.insideUnitSphere * WorldRadius;
			nodes[nodeLoader.LoadNode(i)] = nodeGO;
		}
	}
	
	void Update () {
		
	}

	private void OnDestroy() {
		nodeLoader.Dispose();
	}
}
