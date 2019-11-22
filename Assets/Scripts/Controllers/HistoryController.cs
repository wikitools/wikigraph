﻿using Model;
using Services.History;
using Services.History.Actions;
using Services.Routes;
using Services.Search;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Controllers {
	public class HistoryController : MonoBehaviour {
		private NodeController nodeController;
		public NetworkController NetworkController { get; private set; }
		private GraphController graphController;
		public HistoryService HistoryService { get; private set; }
		public GameObject RouteTemplate;
		public GameObject RouteParent;
		public GameObject SearchTemplateArticle;
		public GameObject SearchTemplateCategory;
		public GameObject SearchParent;
		public int secondsToChangeRoute = 8;
		public int numberOfDisplayedSearchEntries = 10;
		public GameObject searchBox;
		public GameObject SearchScrollView;
		public Action startRouteAutoAction;
		public Action endRouteAutoAction;
		bool graphModeChangedByHistory;
		GameObject[] routesTiles;
		List<GameObject> searchTiles = new List<GameObject>();
		int searchIndex;
		int routeIndex = -1;
		IEnumerator autoRouteCoroutine;
		IEnumerator searchCoroutine;
		bool isSearching = false;
		string searched = "";
		string routesPath;
		string searchFilePath;
		NodeChangedSource nodeChangedSource = NodeChangedSource.User;
		string ROUTES_DIR = "Routes";

		enum NodeChangedSource {
			User,
			History,
			Route,
			Search
		}

		void Awake() {
			NetworkController = GetComponent<NetworkController>();
			nodeController = GetComponent<NodeController>();
			graphController = GetComponent<GraphController>();
		}

		private void Start() {
			if (NetworkController.IsServer()) {
				routesPath = Path.Combine(nodeController.NodeLoadManager.NodeLoader.fileReader.GetDataPackDirectory(), ROUTES_DIR);
				searchFilePath = nodeController.NodeLoadManager.NodeLoader.fileReader.GetDataPackFile() + "s";

				HistoryService = new HistoryService(this, secondsToChangeRoute, numberOfDisplayedSearchEntries, routesPath, searchFilePath);
				nodeController.OnSelectedNodeChanged += (oldNode, newNode) => {
					if (nodeChangedSource != NodeChangedSource.History) {
						HistoryService.RegisterAction(new NodeSelectedAction(oldNode?.ID, newNode?.ID, false));						
					}
					if (nodeChangedSource != NodeChangedSource.Route && isPlayingRoute()) endRouteAutoAction();
					nodeChangedSource = NodeChangedSource.User;
				};
				NodeSelectedAction.selectNodeAction = (node, isRoute) => {
					if (isRoute) nodeChangedSource = NodeChangedSource.Route;
					else nodeChangedSource = NodeChangedSource.History;
					SelectNode(node);
				};
				graphController.ConnectionMode.OnValueChanged += mode => {
					if (nodeChangedSource != NodeChangedSource.History) {
						HistoryService.RegisterAction(new ModeChangeAction<ConnectionMode>(mode, false));
					}
					if (nodeChangedSource != NodeChangedSource.Route && isPlayingRoute()) endRouteAutoAction();
					nodeChangedSource = NodeChangedSource.User;
				};
				ModeChangeAction<ConnectionMode>.changeMode = (mode, isRoute) => {
					if (isRoute) nodeChangedSource = NodeChangedSource.Route;
					else nodeChangedSource = NodeChangedSource.History;
					NetworkController.SetConnectionMode(mode);
				};
				startRouteAutoAction += () => {
					nodeChangedSource = NodeChangedSource.Route;
					autoRouteCoroutine = HistoryService.autoRoutes();
					StartCoroutine(autoRouteCoroutine);
				};
				endRouteAutoAction += () => {
					makeDefaultColorOnRouteTile();
					StopCoroutine(autoRouteCoroutine);
					HistoryService.stopPlayingRoute();
				};
				SearchReader.onIndexRead = index => {
					createSearchObjects(index);
					isSearching = false;
				};

				createRoutesObjects();
			};
		}
		#region RouteHandling

		public bool isPlayingRoute() {
			return HistoryService.isPlayingRoute();
		}

		public void createRoutesObjects() {
			int i = 0;
			int[] lengths = HistoryService.routesLoader.routeReader.lengthOfRoutes();
			routesTiles = new GameObject[lengths.Length];
			foreach (string name in HistoryService.routesLoader.routeReader.namesOfRoutes()) {
				routesTiles[i] = Instantiate(RouteTemplate, RouteParent.transform);
				string getFileName = Path.GetFileNameWithoutExtension(name);
				routesTiles[i].transform.GetChild(0).GetComponent<Text>().text = getFileName;
				routesTiles[i].transform.GetChild(1).GetComponent<Text>().text = "Route Length: <color=black>" + lengths[i] + "</color>";
				routesTiles[i].transform.GetChild(2).name = i.ToString();
				routesTiles[i].transform.GetChild(2).GetComponent<Button>().onClick.AddListener(() => onRouteButtonClicked());
				routesTiles[i].transform.position = routesTiles[i].transform.position + new Vector3(0, -64 * i, 0);
				i++;
			}
		}

		public void onRouteButtonClicked() {
			if (HistoryService.isPlayingRoute()) endRouteAutoAction();
			int newIndex;
			if (Int32.TryParse(EventSystem.current.currentSelectedGameObject.name, out newIndex)) {
				if (newIndex != routeIndex) {
					routeIndex = newIndex;
					HistoryService.startRoute(routeIndex);
					routesTiles[routeIndex].transform.GetComponent<Image>().color = new Color(0.341f, 0.58f, 0.808f, 1.0f);
					routesTiles[routeIndex].transform.GetChild(2).GetComponent<Button>().transform.GetChild(0).GetComponent<Text>().text = "Stop";
				}
				else {
					routeIndex = -1;
				}

			}
		}

		public void makeDefaultColorOnRouteTile() {
			routesTiles[routeIndex].transform.GetComponent<Image>().color = new Color(0.91f, 0.91f, 0.91f, 0.404f);
			routesTiles[routeIndex].transform.GetChild(2).GetComponent<Button>().transform.GetChild(0).GetComponent<Text>().text = "Start";
		}

	
		#endregion

		#region SearchHandling
		public void createSearchObjects(long index) {
			deleteAllSearchEntries();
			Dictionary<uint, string> searchResults = HistoryService.searchLoader.getEntries(index);
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

		private void SelectNode(uint? index) {
			if (index != null) {
				nodeController.NodeLoadManager.LoadNode(index.Value);
				nodeController.OnNodeLoadSessionEnded?.Invoke();
			}
			NetworkController.SetSelectedNode(index.ToString());
		}

		public void OnSearchEntryClicked() {
			uint index;
			if (uint.TryParse(EventSystem.current.currentSelectedGameObject.name, out index)) {
				nodeChangedSource = NodeChangedSource.Search;
				SelectNode(index);
			}
		}

		public void OnSearchTextChanged() {
			string text = searchBox.GetComponent<InputField>().text;
			text = text.Replace(" ", "_");
			if (searched != text) {
				searched = text;
				if (isSearching) StopCoroutine(searchCoroutine);
				if (text != string.Empty) {
					searchCoroutine = HistoryService.searchLoader.reader.BinSearch(text);
					isSearching = true;
					StartCoroutine(searchCoroutine);
					ScrollToTop(SearchScrollView.GetComponent<ScrollRect>());
				}
				else deleteAllSearchEntries();
			}
		}

		private void ScrollToTop(ScrollRect scrollRect) {
			scrollRect.normalizedPosition = new Vector2(0, 1);
		}
		#endregion
	}
}
