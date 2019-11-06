using System;
using UnityEngine;
using UnityEngine.UI;
using Model;

namespace Controllers {
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

		// Use this for initialization
		void Awake() {
			networkController = Graph.GetComponent<NetworkController>();
			inputController = Graph.GetComponent<InputController>();
			nodeController = Graph.GetComponent<NodeController>();
			connectionController = Graph.GetComponent<ConnectionController>();
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
			if(!networkController.IsServer())
				return;
			canvas.enabled = !canvas.enabled;
			Header.SetActive(!canvas.enabled);
			inputController.SetBlockInput(canvas.enabled);
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