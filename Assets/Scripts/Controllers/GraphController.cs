using System.Linq;
using System.Collections.Generic;
using Model;
using Services;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

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
			LoadNode(i);
		}
	}

	private void LoadNode(uint id) {
		Node node = nodeLoader.LoadNode(id);
		GameObject nodeGO = nodePool.Spawn();
		nodeGO.transform.parent = transform;
		nodeGO.transform.position = Random.insideUnitSphere * WorldRadius;
		nodeGO.GetComponentInChildren<Text>().text = node.Title;
		nodes[node] = nodeGO;
	}
	
	void Update () {
		
	}

	private void OnDestroy() {
		nodeLoader.Dispose();
	}
}
