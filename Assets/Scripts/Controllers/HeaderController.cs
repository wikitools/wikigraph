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
		private Vector3 targetPrimaryRangePosition, targetSecondaryRangePosition;
		private Vector2 targetPrimaryRangeSize, targetSecondaryRangeSize;
		private SpriteRenderer headerIndicatorPrimaryRangeSprite, headerIndicatorSecondaryRangeSprite;
		private float indicatorWidth, indicatorHeight;

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
			HeaderObject.transform.GetChild(0).GetComponent<MeshRenderer>().sortingOrder = 50;
			HeaderObject.transform.GetChild(1).GetComponent<MeshRenderer>().sortingOrder = 50;
			HeaderObject.transform.GetChild(2).GetComponent<MeshRenderer>().sortingOrder = 50;
		}

		void Awake() {
			networkController = GetComponent<NetworkController>();
			inputController = GetComponent<InputController>();
			nodeController = GetComponent<NodeController>();
            connectionController = GetComponent<ConnectionController>();
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
				} else {
					headerTitle.text = Config.CurrentlySelectedText;
					headerValue.text = nodeController.SelectedNode.Title;
				}
            } else {
                connectionController.OnConnectionRangeChanged?.Invoke(0, 0, 0);
            }
        }

        private void UpdateNodeHeaderAfterConnectionRangeChange(int start, int end, int count) {
            TextMesh headerConnectionsRangeText = HeaderObject.transform.GetChild(2).GetComponent<TextMesh>();

			if (nodeController.SelectedNode != null && count > 0) {
				HeaderObject.transform.GetChild(3).gameObject.SetActive(true);
				
				// Primary range size & position
				float primaryRangeWidth = (float)(end > start ? end - start + 1 : count - start + 1) / count * indicatorWidth;
				targetPrimaryRangeSize = new Vector2(primaryRangeWidth, indicatorHeight);
				targetPrimaryRangePosition = new Vector3(-(indicatorWidth - primaryRangeWidth) / 2.0f + ((start-1) * indicatorWidth / count), 0, 0);

				if (end <= start) {
					// Second range size & position
					float secondRangeWidth = (float)end / count * 12.0f;
					targetSecondaryRangeSize = new Vector2(secondRangeWidth, indicatorHeight);
					targetSecondaryRangePosition = new Vector3(-(indicatorWidth - secondRangeWidth) / 2.0f, 0, 0);
				} else {
					targetSecondaryRangeSize = new Vector2(0, indicatorHeight);
					targetSecondaryRangePosition = new Vector3(-indicatorWidth / 2.0f, 0, 0);
				}

				// Connections range text
				if (count <= connectionController.ConnectionDistribution.MaxVisibleNumber) {
					headerConnectionsRangeText.text = string.Format("{0} <color=white>{1} {2}</color>", Config.CurrentConnectionRangeText, Config.AllConnectionRangeText, count);
				} else {
					headerConnectionsRangeText.text = string.Format("{0} <color=white>[{1}-{2}/{3}]</color>", Config.CurrentConnectionRangeText, start, end, count);
				}
            } else {
				HeaderObject.transform.GetChild(3).gameObject.SetActive(false);
				headerConnectionsRangeText.text = string.Empty;
            }
        }

		void Update() {
			if (networkController.IsClient()) {
				return;
			}
			
			// Connection indicator update (commented out partially working animation)
			headerIndicatorPrimaryRangeSprite.size = targetPrimaryRangeSize;
			HeaderObject.transform.GetChild(3).GetChild(0).localPosition = targetPrimaryRangePosition;
			headerIndicatorSecondaryRangeSprite.size = targetSecondaryRangeSize;
			HeaderObject.transform.GetChild(3).GetChild(1).localPosition = targetSecondaryRangePosition;
			//headerIndicatorPrimaryRangeSprite.size = Vector2.Lerp(headerIndicatorPrimaryRangeSprite.size, targetPrimaryRangeSize, Time.deltaTime * 10);
			//HeaderObject.transform.GetChild(3).GetChild(0).localPosition = Vector3.Lerp(HeaderObject.transform.GetChild(3).GetChild(0).localPosition, targetPrimaryRangePosition, Time.deltaTime * 10);
			//headerIndicatorSecondaryRangeSprite.size = Vector2.Lerp(headerIndicatorSecondaryRangeSprite.size, targetSecondaryRangeSize, Time.deltaTime * 10);
			//HeaderObject.transform.GetChild(3).GetChild(1).localPosition = Vector3.Lerp(HeaderObject.transform.GetChild(3).GetChild(1).localPosition, targetSecondaryRangePosition, Time.deltaTime * 10);

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