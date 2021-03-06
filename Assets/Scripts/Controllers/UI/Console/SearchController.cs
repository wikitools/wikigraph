﻿using System.Collections;
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

		public GameObject SearchTemplateArticle;
		public GameObject SearchTemplateCategory;
		public GameObject SearchParent;
		public GameObject searchBox;
		public GameObject SearchScrollView;
		public GameObject EmptySetHint;
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
				string entryText = result.Value.Replace("_", " ") + $" <color=#00000040><size=16>{result.Key}</size></color>";
				searchTiles[i].transform.GetChild(1).GetComponent<Text>().text = entryText;
				searchTiles[i].GetComponent<Button>().onClick.AddListener(() => OnSearchEntryClicked());
				searchTiles[i].name = result.Key.ToString();
				i++;
			}
			if(i>0) {
				EmptySetHint.SetActive(false);
			}
		}

		private void deleteAllSearchEntries() {
			foreach (var entry in searchTiles) {
				Destroy(entry);
			}
			searchTiles.Clear();
			EmptySetHint.SetActive(true);
		}

		public void OnSearchEntryClicked() {
			uint index;
			if (uint.TryParse(EventSystem.current.currentSelectedGameObject.name, out index)) {
				actionController.nodeChangedSource = ActionController.NodeChangedSource.Search;
				actionController.SelectNode(index);
				networkController.ToggleConsole();
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