using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Model;

namespace Controllers {
	public class ConsoleWindowController : MonoBehaviour {

		public GameObject ConsoleWindowCanvas;

		private bool isServer;
		private NetworkController networkController;
		private InputController inputController;
		private NodeController nodeController;
		private ConnectionController connectionController;

		// Use this for initialization
		void Awake() {
			networkController = GetComponent<NetworkController>();
			inputController = GetComponent<InputController>();
			nodeController = GetComponent<NodeController>();
			connectionController = GetComponent<ConnectionController>();
			isServer = networkController.IsServer();
		}

		// Use this for initialization
		void Start() {

		}

		// Update is called once per frame
		void Update() {

		}
	}
}