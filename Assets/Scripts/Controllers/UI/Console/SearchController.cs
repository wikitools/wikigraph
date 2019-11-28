using System.Collections;
using System.Collections.Generic;
using Model;
using Services.Search;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Controllers.UI.Console {
	public class SearchController : MonoBehaviour {
		private NodeController nodeController;
		private ActionController actionController;
		private NetworkController networkController;
		private ConsoleWindowController consoleWindow;

		public GameObject SearchTemplateArticle;
		public GameObject SearchTemplateCategory;
		public GameObject SearchParent;
		public GameObject searchBox;
		public GameObject SearchScrollView;
		public GameObject console;
		List<GameObject> searchTiles = new List<GameObject>();

		int searchIndex;
		public int numberOfDisplayedSearchEntries = 10;
		IEnumerator searchCoroutine;
		bool isSearching = false;
		string searched = "";
		const string FILE_EXTENSION_LETTER = "s";
		public SearchLoader searchLoader;
		

		void Start() {
			if (networkController.IsServer()) {
				SearchReader.onIndexRead = index => {
					createSearchObjects(index);
					isSearching = false;
				};
				string path = nodeController.NodeLoadManager.NodeLoader.fileReader.GetDataPackFile() + FILE_EXTENSION_LETTER;
				searchLoader = new SearchLoader(numberOfDisplayedSearchEntries, path);
				consoleWindow = console.GetComponent<ConsoleWindowController>();

			}
		}

		private void Awake() {
			nodeController = GetComponent<NodeController>();
			actionController = GetComponent<ActionController>();
			networkController = GetComponent<NetworkController>();
		}

		public void createSearchObjects(long index) {
			deleteAllSearchEntries();
			Dictionary<uint, string> searchResults = searchLoader.getEntries(index);
			int i = 0;
			foreach (var result in searchResults) {
				if (nodeController.NodeLoadManager.NodeLoader.GetNodeType(result.Key) == NodeType.ARTICLE) {
					searchTiles.Add(Instantiate(SearchTemplateArticle, SearchParent.transform));
				}
				else {
					searchTiles.Add(Instantiate(SearchTemplateCategory, SearchParent.transform));
				}
				searchTiles[i].transform.GetChild(1).GetComponent<Text>().text = result.Value.Replace("_", " ");
				searchTiles[i].GetComponent<Button>().onClick.AddListener(() => OnSearchEntryClicked());
				searchTiles[i].transform.GetChild(searchTiles[i].transform.childCount - 1).GetComponent<Text>().text = "ID: <color=black>" + result.Key + "</color>";
				searchTiles[i].name = result.Key.ToString();
				i++;
			}
		}

		private void deleteAllSearchEntries() {
			foreach (var entry in searchTiles) {
				Destroy(entry);
			}
			searchTiles.Clear();
		}

		public void OnSearchEntryClicked() {
			uint index;
			if (uint.TryParse(EventSystem.current.currentSelectedGameObject.name, out index)) {
				actionController.nodeChangedSource = ActionController.NodeChangedSource.Search;
				actionController.SelectNode(index);
				consoleWindow.ToggleVisibility();
			}
		}

		public void OnSearchTextChanged() {
			string text = searchBox.GetComponent<InputField>().text;
			text = text.Replace(" ", "_");
			if (searched != text) {
				searched = text;
				if (isSearching) {
					StopCoroutine(searchCoroutine);
				}

				if (text != string.Empty) {
					searchCoroutine = searchLoader.reader.BinSearch(text);
					isSearching = true;
					StartCoroutine(searchCoroutine);
					ScrollToTop(SearchScrollView.GetComponent<ScrollRect>());
				}
				else {
					deleteAllSearchEntries();
				}
			}
		}
		private void ScrollToTop(ScrollRect scrollRect) {
			scrollRect.normalizedPosition = new Vector2(0, 1);
		}
	}
}