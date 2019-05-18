using System;
using Services;
using UnityEngine;

public class GraphController : MonoBehaviour {
	private DataFileReader fileReader;
	
	void Start () {
		fileReader = new DataFileReader();
	}
	
	void Update () {
		
	}

	private void OnDestroy() {
		fileReader.Dispose();
	}
}
