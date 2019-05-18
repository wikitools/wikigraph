using Services;
using UnityEngine;

public class GraphController : MonoBehaviour {
	private NodeLoader nodeLoader;
	
	void Start () {
		nodeLoader = new NodeLoader();
	}
	
	void Update () {
		
	}

	private void OnDestroy() {
		nodeLoader.Dispose();
	}
}
