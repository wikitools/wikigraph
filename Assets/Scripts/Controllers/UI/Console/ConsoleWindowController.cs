using System;
using Inspector;
using Model;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Controllers.UI.Console {
	public class ConsoleWindowController : MonoBehaviour {

		public GameObject Graph;
		public GameObject Header;
		public GameObject RoutesContent;
		public ConsoleWindowConfig Config;

		private Canvas canvas;
		private Text headerHint, headerTitle;
		private Image headerIcon;
		private Toggle headerTabSearch, headerTabRoutes;
		private InputField searchInput;

		private NetworkController networkController;
		private InputController inputController;
		private NodeController nodeController;
		private ConnectionController connectionController;
		private HeaderController headerController;

		private bool active;
		public Action<bool> OnConsoleToggled;

		// Use this for initialization
		void Awake() {
			networkController = Graph.GetComponent<NetworkController>();
			inputController = Graph.GetComponent<InputController>();
			nodeController = Graph.GetComponent<NodeController>();
			connectionController = Graph.GetComponent<ConnectionController>();
			headerController = Header.GetComponent<HeaderController>();
		}

		void Start() {
			headerHint = transform.Find("ConsoleHeader/NodeHint").GetComponent<Text>();
			headerTitle = transform.Find("ConsoleHeader/NodeName").GetComponent<Text>();
			headerIcon = transform.Find("ConsoleHeader/NodeIcon").GetComponent<Image>();
			headerTabSearch = transform.Find("TabPanel/TabContainer/TabSearch").GetComponent<Toggle>();
			headerTabRoutes = transform.Find("TabPanel/TabContainer/TabRoutes").GetComponent<Toggle>();
			searchInput = transform.Find("TabPanel/SearchContent/SearchInput").GetComponent<InputField>();
			canvas = gameObject.GetComponent<Canvas>();
			headerHint.text = Config.CurrentlySelectedText;
			nodeController.OnSelectedNodeChanged += UpdateNodeHeaderAfterSelect;
			UpdateNodeHeaderAfterSelect(null, null);

			if (networkController.IsServer())
				Cursor.visible = true;
		}

		private void UpdateNodeHeaderAfterSelect(Node previousNode, Node selectedNode) {
			headerTitle.text = selectedNode != null ? selectedNode.Title : Config.NothingSelectedText;
			headerIcon.sprite = selectedNode != null ? (selectedNode.Type == NodeType.ARTICLE ? Config.ArticleIconSprite : Config.CategoryIconSprite) : Config.MaskSprite;
		}

		public void ToggleVisibility() {
			active = !active;
			OnConsoleToggled?.Invoke(active);
			if(!networkController.IsServer())
				return;
			canvas.enabled = active;
			headerController.SetEnabled(!active);
			inputController.SetBlockInput(canvas.enabled, InputBlockType.CONSOLE);
			if (active) {
				if (headerTabSearch.isOn) {
					SelectSearchInput();
				} else {
					SelectFirstRoute();
				}
			} else {
				DeselectSearchInput();
			}
		}

		public bool GetActive() {
			return active;
		}

		void SelectSearchInput() {
			searchInput.ActivateInputField();
			searchInput.Select();
		}

		void DeselectSearchInput() {
			searchInput.DeactivateInputField();
		}

		void SelectFirstRoute() {
			if (RoutesContent.transform.childCount > 0)
				EventSystem.current.SetSelectedGameObject(RoutesContent.transform.GetChild(0).GetComponent<Button>().gameObject);
		}

		void Update() {
			if (active) {
				// Tab change
				if (Input.GetKeyDown(KeyCode.Tab)) {
					if (headerTabSearch.isOn) {
						headerTabSearch.isOn = false;
						headerTabRoutes.isOn = true;
						SelectFirstRoute();
					} else {
						headerTabSearch.isOn = true;
						headerTabRoutes.isOn = false;
						SelectSearchInput();
					}
				}
				// Search input exit with arrow down
				if (Input.GetKeyDown(KeyCode.DownArrow)) {
					if (headerTabSearch.isOn) {
						searchInput.OnDeselect(new BaseEventData(EventSystem.current));
					}
				}
			}
		}

		[Serializable]
		public class ConsoleWindowConfig {
			public Sprite ArticleIconSprite;
			public Sprite CategoryIconSprite;
			public Sprite MaskSprite;
			public string NothingSelectedText = "Nothing";
			public string CurrentlySelectedText = "Currently selected:";
		}
	}
}