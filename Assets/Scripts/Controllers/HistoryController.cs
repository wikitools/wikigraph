using Model;
using Services.History;
using Services.History.Actions;
using Services.RoutesFiles;
using Services.SearchFiles;
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
		private NetworkController networkController;
		private GraphController graphController;
		public HistoryService HistoryService { get; private set; }
		public static Action startLoading;
		public GameObject RouteTemplate;
		public GameObject RouteParent;
		public GameObject SearchTemplateArticle;
		public GameObject SearchTemplateCategory;
		public GameObject SearchParent;
		public int secondsToChangeRoute = 8;
		public int numberOfDisplayedSearchEntries = 10;
		public GameObject searchBox;

		bool nodeChangedByHistory;
		bool connectionModeChangedByHistory;
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

		string ROUTES_DIR = "Routes";

		void Awake() {
			networkController = GetComponent<NetworkController>();
			nodeController = GetComponent<NodeController>();
			graphController = GetComponent<GraphController>();
		}

		private void Start() {
			if (networkController.IsServer()) {
				routesPath = Path.Combine(nodeController.NodeLoadManager.NodeLoader.fileReader.GetDataPackDirectory(), ROUTES_DIR);
				searchFilePath = nodeController.NodeLoadManager.NodeLoader.fileReader.GetDataPackFile() + "s";

				HistoryService = new HistoryService(secondsToChangeRoute, numberOfDisplayedSearchEntries, routesPath, searchFilePath);
				nodeController.OnSelectedNodeChanged += (oldNode, newNode) => {
					if (!nodeChangedByHistory) {
						HistoryService.RegisterAction(new NodeSelectedAction(oldNode, newNode));
						if (isPlayingRoute()) onRouteExit();
					}
					nodeChangedByHistory = false;
					
				};
				NodeSelectedAction.selectNodeAction = node => {
					nodeChangedByHistory = true;
					networkController.SetSelectedNode(node);
				};
				graphController.ConnectionMode.OnValueChanged += mode => {
					if (!connectionModeChangedByHistory) HistoryService.RegisterAction(new ModeChangeAction<ConnectionMode>(mode));
					connectionModeChangedByHistory = false;
				};
				ModeChangeAction<ConnectionMode>.changeMode = mode => {
					connectionModeChangedByHistory = true;
					networkController.SetConnectionMode(mode);
				};
				RoutesLoader.getRouteNode = id => {
					return nodeController.NodeLoadManager.LoadNode(id);
				};
				HistoryService.startRouteAutoAction += () => {
					autoRouteCoroutine = HistoryService.autoRoutes();
					StartCoroutine(autoRouteCoroutine);
				};
				HistoryService.endRouteAutoAction += () => {
					makeDefaultColorOnRouteTile();
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
			int[] lengths = HistoryService.getLengths();
			routesTiles = new GameObject[lengths.Length];
			foreach (string name in HistoryService.getNames()) {
				routesTiles[i] = Instantiate(RouteTemplate, RouteParent.transform);
				string[] getFileName = name.Split('\\');
				routesTiles[i].transform.GetChild(0).GetComponent<Text>().text = getFileName[getFileName.Length - 1].Split('.')[0];
				routesTiles[i].transform.GetChild(1).GetComponent<Text>().text = "Route Length: <color=black>" + lengths[i].ToString() + "</color>";
				routesTiles[i].transform.GetChild(2).name = i.ToString();
				routesTiles[i].transform.GetChild(2).GetComponent<Button>().onClick.AddListener(() => onRouteButtonClicked());
				routesTiles[i].transform.position = routesTiles[i].transform.position + new Vector3(0, -64 * i, 0);
				i++;
			}
		}

		public void onRouteButtonClicked() {
			if (HistoryService.isPlayingRoute()) onRouteExit();
			int newIndex;
			if (Int32.TryParse(EventSystem.current.currentSelectedGameObject.name, out newIndex)) {
				if(newIndex != routeIndex) {
					routeIndex = newIndex;
					HistoryService.startRoute(routeIndex);
					routesTiles[routeIndex].transform.GetComponent<Image>().color = new Color(0.341f, 0.58f, 0.808f, 1.0f);
					routesTiles[routeIndex].transform.GetChild(2).GetComponent<Button>().transform.GetChild(0).GetComponent<Text>().text = "Stop";
				}
				
			}
		}

		public void makeDefaultColorOnRouteTile() {
			routesTiles[routeIndex].transform.GetComponent<Image>().color = new Color(0.91f, 0.91f, 0.91f, 0.404f);
			routesTiles[routeIndex].transform.GetChild(2).GetComponent<Button>().transform.GetChild(0).GetComponent<Text>().text = "Start";
		}

		public void onRouteExit() {
			makeDefaultColorOnRouteTile();
			StopCoroutine(autoRouteCoroutine);
			HistoryService.stopPlayingRoute();
		}
		#endregion

		#region SearchHandling
		public void createSearchObjects(long index) {
			
			deleteAllSearchEntries();
			Dictionary<uint, string> searchResults = HistoryService.searchLoader.getEntries(index);
			int i = 0;
			foreach (var result in searchResults) {
				Node node = nodeController.NodeLoadManager.LoadNode(result.Key);
				if(node.Type == NodeType.ARTICLE) {
					searchTiles.Add(Instantiate(SearchTemplateArticle, SearchParent.transform));
				} else {
					searchTiles.Add(Instantiate(SearchTemplateCategory, SearchParent.transform));
				}
				var t = searchTiles[i].transform.GetChild(1).GetComponent<Text>().text = result.Value.Replace("_", " ");
				searchTiles[i].GetComponent<Button>().onClick.AddListener(() => OnSearchEntryClicked());
				searchTiles[i].name = result.Key.ToString();
				i++;

			}
			
		}

		private void deleteAllSearchEntries() {
			foreach(var entry in searchTiles) {
				Destroy(entry);
			}
			searchTiles.Clear();
		}

		public void OnSearchEntryClicked() {
			uint index;
			if (uint.TryParse(EventSystem.current.currentSelectedGameObject.name, out index)) {
				nodeChangedByHistory = true;
				Node node = nodeController.NodeLoadManager.LoadNode(index);
				networkController.SetSelectedNode(node);
			}
		}

		public void OnSearchTextChanged() {
			string text = searchBox.GetComponent<InputField>().text;
			text = text.Replace(" ", "_");
			if(searched != text) {
				searched = text;
				if (isSearching) StopCoroutine(searchCoroutine);
				if (text != string.Empty) {
					searchCoroutine = HistoryService.searchLoader.reader.BinSearch(text);
					isSearching = true;
					StartCoroutine(searchCoroutine);
				}
			}
			
				 

		}

		#endregion
	}
}