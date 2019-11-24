using System;
using Inspector;
using Model;
using UnityEngine;
using UnityEngine.UI;

namespace Controllers.UI.Console {
	public class ConsoleWindowController : MonoBehaviour {

		public GameObject Graph;
		public GameObject Header;
		public ConsoleWindowConfig Config;

		private Canvas canvas;
		private Text headerHint, headerTitle;
		private Image headerIcon;

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
			canvas = gameObject.GetComponent<Canvas>();
			headerHint.text = Config.CurrentlySelectedText;
			nodeController.OnSelectedNodeChanged += UpdateNodeHeaderAfterSelect;
			UpdateNodeHeaderAfterSelect(null, null);
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
		}

		public bool GetActive() {
			return active;
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