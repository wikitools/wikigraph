using System;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine;
using Model;
using System.Linq;

namespace Controllers {
	public class HeaderController : MonoBehaviour {
		public GameObject Entity;
		public GameObject HeaderObject;
		public HeaderConfig Config;

		private Vector3 targetPosition;
		private Vector3 targetPrimaryRangePosition, targetSecondaryRangePosition;
		private Vector2 targetPrimaryRangeSize, targetSecondaryRangeSize;
		private SpriteRenderer headerIndicatorPrimaryRangeSprite, headerIndicatorSecondaryRangeSprite;
		private float indicatorWidth, indicatorHeight;
		private int currentStart, currentEnd, currentCount;

		private InputController inputController;
		private NodeController nodeController;
		private NetworkController networkController;
		private ConnectionController connectionController;

		void Start() {
			if (inputController.Environment == Environment.PC) {
				HeaderObject.transform.parent = Camera.main.transform;
			}
			nodeController.OnSelectedNodeChanged += UpdateNodeHeaderAfterSelectOrHighlight;
			nodeController.OnHighlightedNodeChanged += UpdateNodeHeaderAfterSelectOrHighlight;
			connectionController.OnConnectionRangeChanged += UpdateNodeHeaderAfterConnectionRangeChange;
			connectionController.OnConnectionRangeChanged?.Invoke(0, 0, 0);

			Transform indicatorBase = HeaderObject.transform.GetChild(3);
			indicatorBase.GetComponent<SpriteRenderer>().size = Config.HeaderRangeSize;
			indicatorWidth = indicatorBase.GetComponent<SpriteRenderer>().size.x;
			indicatorHeight = indicatorBase.GetComponent<SpriteRenderer>().size.y;
			headerIndicatorPrimaryRangeSprite = indicatorBase.GetChild(0).GetComponent<SpriteRenderer>();
			headerIndicatorSecondaryRangeSprite = indicatorBase.GetChild(1).GetComponent<SpriteRenderer>();

			// Render always on top of nodes and connections
			for (int i = 0; i < 3; i++) {
				SetRendererSortingOrder(HeaderObject.transform.GetChild(i), 50);
			}
		}

		void Awake() {
			networkController = GetComponent<NetworkController>();
			inputController = GetComponent<InputController>();
			nodeController = GetComponent<NodeController>();
			nodeController = GetComponent<NodeController>();
			connectionController = GetComponent<ConnectionController>();
		}

		private void SetRendererSortingOrder(Transform obj, int order) {
			obj.GetComponent<MeshRenderer>().sortingOrder = order;
		}

		private void UpdateNodeHeaderAfterSelectOrHighlight(Node previousNode, Node selectedNode) {
			TextMesh headerTitle = HeaderObject.transform.GetChild(0).GetComponent<TextMesh>();
			TextMesh headerValue = HeaderObject.transform.GetChild(1).GetComponent<TextMesh>();

			if (nodeController.HighlightedNode != null) {
				headerTitle.text = Config.CurrentlyLookingAtText;
				headerValue.text = nodeController.HighlightedNode.Title;
			} else {
				headerTitle.text = string.Empty;
				headerValue.text = string.Empty;
			}
			if (nodeController.SelectedNode != null) {
				if (nodeController.HighlightedNode != null && nodeController.HighlightedNode != nodeController.SelectedNode) {
					headerTitle.text = Config.CurrentlyLookingAtText;
					headerValue.text = nodeController.HighlightedNode.Title;
					ShowConnectionRangeCount(connectionController.GetNodeNeighbours(nodeController.HighlightedNode).ToArray().Length);
				} else {
					headerTitle.text = Config.CurrentlySelectedText;
					headerValue.text = nodeController.SelectedNode.Title;
					ShowConnectionRangeCount(null);
				}
			} else {
				connectionController.OnConnectionRangeChanged?.Invoke(0, 0, 0);
				if (nodeController.HighlightedNode != null) {
					ShowConnectionRangeCount(connectionController.GetNodeNeighbours(nodeController.HighlightedNode).ToArray().Length);
				}
			}
		}

		private void ShowConnectionRangeCount(int? count) {
			ConnectionRangeTextUpdate(count);
			HeaderObject.transform.GetChild(2).localPosition = new Vector3(0, (count != null) ? 15.6f : 15f, 4.5f);
			HeaderObject.transform.GetChild(3).gameObject.SetActive(count == null);
		}

		private void ConnectionRangeTextUpdate(int? count) {
			TextMesh headerConnectionsRangeText = HeaderObject.transform.GetChild(2).GetComponent<TextMesh>();
			string textPrepend = $"{Config.CurrentConnectionRangeText} <color=white>";
			string textAppend = $"</color>";

			if (count != null) {
				headerConnectionsRangeText.text = textPrepend + $"{count}" + textAppend;
			} else {
				if (currentCount <= connectionController.ConnectionDistribution.MaxVisibleNumber) {
					headerConnectionsRangeText.text = textPrepend + $"{Config.AllConnectionRangeText} {currentCount}" + textAppend;
				} else {
					headerConnectionsRangeText.text = textPrepend + $"[{currentStart}-{currentEnd}/{currentCount}]" + textAppend;
				}
			}
		}

		private void UpdateNodeHeaderAfterConnectionRangeChange(int start, int end, int count) {
			TextMesh headerConnectionsRangeText = HeaderObject.transform.GetChild(2).GetComponent<TextMesh>();

			if (nodeController.SelectedNode != null && count > 0) {
				HeaderObject.transform.GetChild(3).gameObject.SetActive(true);

				// Primary range size & position
				float primaryRangeWidth = (float)(end > start ? end - start + 1 : count - start + 1) / count * indicatorWidth;
				targetPrimaryRangeSize = new Vector2(primaryRangeWidth, indicatorHeight);
				targetPrimaryRangePosition = new Vector3(-(indicatorWidth - primaryRangeWidth) / 2.0f + ((start - 1) * indicatorWidth / count), 0, 0);

				if (end <= start) {
					// Second range size & position
					float secondRangeWidth = (float)end / count * 12.0f;
					targetSecondaryRangeSize = new Vector2(secondRangeWidth, indicatorHeight);
					targetSecondaryRangePosition = new Vector3(-(indicatorWidth - secondRangeWidth) / 2.0f, 0, 0);
				} else {
					targetSecondaryRangeSize = new Vector2(0, indicatorHeight);
					targetSecondaryRangePosition = new Vector3(-indicatorWidth / 2.0f, 0, 0);
				}

				currentStart = start;
				currentEnd = end;
				currentCount = count;
				ShowConnectionRangeCount(null);
			} else {
				HeaderObject.transform.GetChild(3).gameObject.SetActive(false);
				headerConnectionsRangeText.text = string.Empty;
			}
		}

		void Update() {
			// Connection indicator update
			headerIndicatorPrimaryRangeSprite.size = targetPrimaryRangeSize;
			HeaderObject.transform.GetChild(3).GetChild(0).localPosition = targetPrimaryRangePosition;
			headerIndicatorSecondaryRangeSprite.size = targetSecondaryRangeSize;
			HeaderObject.transform.GetChild(3).GetChild(1).localPosition = targetSecondaryRangePosition;

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
			public Vector2 HeaderRangeSize = new Vector2(12f, 0.25f);
			public string CurrentlySelectedText = "Selected:";
			public string CurrentlyLookingAtText = "Looking at:";
			public string CurrentConnectionRangeText = "Connections:";
			public string AllConnectionRangeText = "All";
		}
	}
}