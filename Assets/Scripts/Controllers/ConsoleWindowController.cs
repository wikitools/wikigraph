using System;
using UnityEngine;
using UnityEngine.UI;
using Model;

namespace Controllers {
	public class ConsoleWindowController : MonoBehaviour {

		public GameObject Graph;
		public GameObject Header;
		public ConsoleWindowConfig Config;

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
			nodeController.OnSelectedNodeChanged += UpdateNodeHeaderAfterSelect;
			UpdateNodeHeaderAfterSelect(null, null);
			Text headerHint = transform.Find("ConsoleHeader/NodeHint").GetComponent<Text>();
			headerHint.text = Config.CurrentlySelectedText;
			gameObject.SetActive(false);
		}

		private void UpdateNodeHeaderAfterSelect(Node previousNode, Node selectedNode) {
			Text headerTitle = transform.Find("ConsoleHeader/NodeName").GetComponent<Text>();
			Image headerIcon = transform.Find("ConsoleHeader/NodeIcon").GetComponent<Image>();
			headerTitle.text = selectedNode != null ? selectedNode.Title : Config.NothingSelectedText;
			headerIcon.sprite = selectedNode != null ? (selectedNode.Type == NodeType.ARTICLE ? Config.ArticleIconSprite : Config.CategoryIconSprite) : Config.MaskSprite;
		}

		public void ToggleVisibility() {
			if(!networkController.IsServer())
				return;
			Header.SetActive(gameObject.activeSelf);
			gameObject.SetActive(!gameObject.activeSelf);
			inputController.SetBlockInput(gameObject.activeSelf);
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