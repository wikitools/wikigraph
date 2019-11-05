using System;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine;
using Model;

namespace Controllers {
	public class HeaderController : MonoBehaviour {
		public GameObject Entity;
		public GameObject HeaderObject;
		public HeaderConfig Config;

		private Vector3 targetPosition;
		private InputController inputController;
		private NodeController nodeController;
		private NetworkController networkController;

		void Start() {
			if (inputController.Environment == Environment.PC) {
				HeaderObject.transform.parent = Camera.main.transform;
			}
			nodeController.OnSelectedNodeChanged += UpdateNodeHeaderAfterSelectOrHighlight;
			nodeController.OnHighlightedNodeChanged += UpdateNodeHeaderAfterSelectOrHighlight;
		}

		void Awake() {
			networkController = GetComponent<NetworkController>();
			inputController = GetComponent<InputController>();
			nodeController = GetComponent<NodeController>();
		}

		private void UpdateNodeHeaderAfterSelectOrHighlight(Node previousNode, Node selectedNode) {
			TextMesh headerTitle = HeaderObject.transform.GetChild(0).GetComponent<TextMesh>();
			TextMesh headerValue = HeaderObject.transform.GetChild(1).GetComponent<TextMesh>();

			if (nodeController.HighlightedNode != null) {
				headerTitle.text = Config.CurrentlyLookingAtText;
				headerValue.text = nodeController.HighlightedNode.Title;
			} else {
				headerTitle.text = "";
				headerValue.text = "";
			}
			if (nodeController.SelectedNode != null) {
				if (nodeController.HighlightedNode != null && nodeController.HighlightedNode != nodeController.SelectedNode) {
					headerTitle.text = Config.CurrentlyLookingAtText;
					headerValue.text = nodeController.HighlightedNode.Title;
				} else {
					headerTitle.text = Config.CurrentlySelectedText;
					headerValue.text = nodeController.SelectedNode.Title;
				}
			}
		}

		void Update() {
			if (networkController.IsClient()) {
				return;
			}

			targetPosition = Entity.transform.position;
			if (inputController.Environment == Environment.Cave) {
				var anglesY = Entity.transform.rotation.eulerAngles.y / 180f * Mathf.PI;
				HeaderObject.transform.position = targetPosition + new Vector3(Mathf.Sin(anglesY) * Config.HeaderDistance,
					                                  Config.HeaderHeight, Mathf.Cos(anglesY) * Config.HeaderDistance);
				HeaderObject.transform.rotation =
					Quaternion.LookRotation(HeaderObject.transform.position - (targetPosition + new Vector3(0, Config.HeaderDeviation, 0)));
			} else {
				HeaderObject.transform.localPosition = Config.HeaderDistance * 2 * Vector3.forward;
			}
		}

		[Serializable]
		public class HeaderConfig {
			public float HeaderHeight = -6f;
			public float HeaderDeviation = -10f;
			public float HeaderDistance = 16f;
			public string CurrentlySelectedText = "Selected:";
			public string CurrentlyLookingAtText = "Looking at:";
		}
	}
}