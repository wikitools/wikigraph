using System;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine;
using Model;
using System.Linq;
using Services.History;
using Services.History.Actions;

namespace Controllers {
	public class HeaderController : MonoBehaviour {

		public GameObject Entity;
		public GameObject Graph;
		public GameObject ConsoleWindow;
		public HeaderConfig Config;
		public HistoryService HistoryService { get; private set; }

		private Vector3 targetPosition;
		private Vector3 targetPrimaryRangePosition, targetSecondaryRangePosition;
		private Vector2 targetPrimaryRangeSize, targetSecondaryRangeSize;
		private SpriteRenderer headerIndicatorPrimaryRangeSprite, headerIndicatorSecondaryRangeSprite;
		private float indicatorWidth, indicatorHeight;
		private int currentStart, currentEnd, currentCount;
		private SpriteRenderer stateIcon;
		private TextMesh autoState;

		private InputController inputController;
		private NodeController nodeController;
		private NetworkController networkController;
		private ConnectionController connectionController;
		private GraphController graphController;
		private ConsoleWindowController consoleWindowController;

		void Start() {
			if (inputController.Environment == Environment.PC) {
				transform.parent = Camera.main.transform;
			}
			nodeController.OnSelectedNodeChanged += UpdateNodeHeaderAfterSelectOrHighlight;
			nodeController.OnHighlightedNodeChanged += UpdateNodeHeaderAfterSelectOrHighlight;
			graphController.ConnectionMode.OnValueChanged += UpdateConnectionMode;
			HistoryService.startRouteAutoAction += UpdateAutoStateAfterRouteChange(true);
			HistoryService.endRouteAutoAction += UpdateAutoStateAfterRouteChange(false);
			consoleWindowController.OnConsoleToggled += UpdateConsoleState;
			connectionController.OnConnectionRangeChanged += UpdateNodeHeaderAfterConnectionRangeChange;
			connectionController.OnConnectionRangeChanged?.Invoke(0, 0, 0);

			Transform indicatorBase = transform.GetChild(3);
			indicatorBase.GetComponent<SpriteRenderer>().size = Config.HeaderRangeSize;
			indicatorWidth = indicatorBase.GetComponent<SpriteRenderer>().size.x;
			indicatorHeight = indicatorBase.GetComponent<SpriteRenderer>().size.y;
			headerIndicatorPrimaryRangeSprite = indicatorBase.GetChild(0).GetComponent<SpriteRenderer>();
			headerIndicatorSecondaryRangeSprite = indicatorBase.GetChild(1).GetComponent<SpriteRenderer>();
			stateIcon = transform.GetChild(4).GetComponent<SpriteRenderer>();

			// Render always on top of nodes and connections
			for (int i = 0; i < 3; i++) {
				SetRendererSortingOrder(transform.GetChild(i), 50);
			}
			SetRendererSortingOrder(transform.GetChild(5), 51);
			SetRendererSortingOrder(transform.GetChild(6).GetChild(1), 51);
			transform.GetChild(6).GetChild(1).GetComponent<TextMesh>().text = Config.ConsoleActiveText;
		}

		void Awake() {
			networkController = Graph.GetComponent<NetworkController>();
			inputController = Graph.GetComponent<InputController>();
			nodeController = Graph.GetComponent<NodeController>();
			connectionController = Graph.GetComponent<ConnectionController>();
			graphController = Graph.GetComponent<GraphController>();
			consoleWindowController = ConsoleWindow.GetComponent<ConsoleWindowController>();
		}

		private void SetRendererSortingOrder(Transform obj, int order) {
			obj.GetComponent<MeshRenderer>().sortingOrder = order;
		}

		public void SetEnabled(bool value) {
			foreach (Transform child in transform) {
				child.gameObject.SetActive(value);
			}
			UpdateConsoleState(consoleWindowController.GetActive());
			if (nodeController.SelectedNode == null) {
				transform.GetChild(3).gameObject.SetActive(false);
			}
		}

		private void UpdateConnectionMode(ConnectionMode mode) {
			if(nodeController.SelectedNode.Type == NodeType.ARTICLE) {
				stateIcon.sprite = (mode == ConnectionMode.PARENTS) ? Config.ArticleConnectionsIn : Config.ArticleConnectionsOut; 
			} else {
				stateIcon.sprite = (mode == ConnectionMode.PARENTS) ? Config.CategoryConnectionsIn : Config.CategoryConnectionsOut;
			}
		}

		private void UpdateConsoleState(bool active) {
			transform.GetChild(6).gameObject.SetActive(active);
		}

		private Action UpdateAutoStateAfterRouteChange(bool started) {
			return () => transform.GetChild(5).GetComponent<TextMesh>().text = started ? Config.AutoText : "";
		}

		private void UpdateNodeHeaderAfterSelectOrHighlight(Node previousNode, Node selectedNode) {
			TextMesh headerTitle = transform.GetChild(0).GetComponent<TextMesh>();
			TextMesh headerValue = transform.GetChild(1).GetComponent<TextMesh>();

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
					stateIcon.sprite = null;
					ShowConnectionRangeCount(connectionController.GetNodeNeighbours(nodeController.HighlightedNode).ToArray().Length);
				} else {
					headerTitle.text = Config.CurrentlySelectedText;
					headerValue.text = nodeController.SelectedNode.Title;
					UpdateConnectionMode(graphController.ConnectionMode.Value);
					ShowConnectionRangeCount(null);
				}
			} else {
				connectionController.OnConnectionRangeChanged?.Invoke(0, 0, 0);
				if (nodeController.HighlightedNode != null) {
					ShowConnectionRangeCount(connectionController.GetNodeNeighbours(nodeController.HighlightedNode).ToArray().Length);
				}
				stateIcon.sprite = null;
			}
		}

		private void ShowConnectionRangeCount(int? count) {
			ConnectionRangeTextUpdate(count);
			transform.GetChild(2).localPosition = new Vector3(0, (count != null) ? 15.6f : 15f, 4.5f);
			transform.GetChild(3).gameObject.SetActive(count == null);
		}

		private void ConnectionRangeTextUpdate(int? count) {
			TextMesh headerConnectionsRangeText = transform.GetChild(2).GetComponent<TextMesh>();
			string textPrepend = $"{Config.CurrentConnectionRangeText} <color=white>";
			string textAppend = $"</color>";

			if (count != null) {
				headerConnectionsRangeText.text = textPrepend + $"{count}" + textAppend;
			} else {
				if (currentCount == 0) {
					headerConnectionsRangeText.text = textPrepend + Config.ZeroConnectionsText + textAppend;
				} else if (currentCount <= connectionController.ConnectionDistribution.MaxVisibleNumber) {
					headerConnectionsRangeText.text = textPrepend + $"{Config.AllConnectionRangeText} {currentCount}" + textAppend;
				} else {
					headerConnectionsRangeText.text = textPrepend + $"[{currentStart}-{currentEnd}/{currentCount}]" + textAppend;
				}
			}
		}

		private void UpdateNodeHeaderAfterConnectionRangeChange(int start, int end, int count) {
			TextMesh headerConnectionsRangeText = transform.GetChild(2).GetComponent<TextMesh>();

			if (nodeController.SelectedNode != null && count >= 0) {
				transform.GetChild(3).gameObject.SetActive(true);

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
				transform.GetChild(3).gameObject.SetActive(false);
				headerConnectionsRangeText.text = string.Empty;
			}
		}
		
		void Update() {
			// Connection indicator update
			headerIndicatorPrimaryRangeSprite.size = targetPrimaryRangeSize;
			transform.GetChild(3).GetChild(0).localPosition = targetPrimaryRangePosition;
			headerIndicatorSecondaryRangeSprite.size = targetSecondaryRangeSize;
			transform.GetChild(3).GetChild(1).localPosition = targetSecondaryRangePosition;

			if (networkController.IsClient()) {
				return;
			}
			
			targetPosition = Entity.transform.position;
			if (inputController.Environment == Environment.Cave) {
				var anglesY = Entity.transform.rotation.eulerAngles.y / 180f * Mathf.PI;
				transform.position = targetPosition + new Vector3(Mathf.Sin(anglesY) * Config.HeaderDistance,
													  Config.HeaderHeight, Mathf.Cos(anglesY) * Config.HeaderDistance);
				transform.rotation =
					Quaternion.LookRotation(transform.position - (targetPosition + new Vector3(0, Config.HeaderDeviation, 0)));
			} else {
				transform.localPosition = Config.HeaderDistance * 2 * Vector3.forward;
				transform.rotation = Quaternion.LookRotation(transform.position - Entity.transform.position);
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
			public string ZeroConnectionsText = "None";
			public string AutoText = "Auto";
			public string ConsoleActiveText = "Console Active";
			public Sprite ArticleConnectionsIn;
			public Sprite ArticleConnectionsOut;
			public Sprite CategoryConnectionsIn;
			public Sprite CategoryConnectionsOut;
		}
	}
}