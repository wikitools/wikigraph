using Services.History;
using Services.History.Actions;
using Services.RoutesFiles;
using System;
using System.Collections;
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


		bool nodeChangedByHistory;
		bool connectionModeChangedByHistory;
		bool graphModeChangedByHistory;
		GameObject[] routesTiles;
		int routeIndex;
		IEnumerator autoRouteCoroutine;

		void Awake() {
			networkController = GetComponent<NetworkController>();
			nodeController = GetComponent<NodeController>();
			graphController = GetComponent<GraphController>();
		}

		private void Start() {
			if (networkController.IsServer()) {
				HistoryService = new HistoryService();
				nodeController.OnSelectedNodeChanged += (oldNode, newNode) => {
					if (!nodeChangedByHistory) HistoryService.RegisterAction(new NodeSelectedAction(oldNode, newNode));
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
				HistoryService.startRouteAutoAction = () => {
					autoRouteCoroutine = HistoryService.autoRoutes();
					StartCoroutine(autoRouteCoroutine);
				};
				HistoryService.endRouteAutoAction = () => {
					makeDefaultColorOnRouteTile();
				};

				createRoutesObjects();
			};
		}

		public bool isPlayingRoute() {
			return HistoryService.isPlayingRoute();
		}

		public void createRoutesObjects() {
			int i = 0;
			int[] lengths = HistoryService.getLengths();
			routesTiles = new GameObject[lengths.Length];
			foreach (string name in HistoryService.getNames()) {
				routesTiles[i] = Instantiate(RouteTemplate, RouteParent.transform);
				string[] getFileName = name.Split('/');
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
			if (Int32.TryParse(EventSystem.current.currentSelectedGameObject.name, out routeIndex)) {
				HistoryService.startRoute(routeIndex);
				routesTiles[routeIndex].transform.GetComponent<Image>().color = new Color(0.341f, 0.58f, 0.808f, 1.0f);
			}
		}

		public void makeDefaultColorOnRouteTile() {
			routesTiles[routeIndex].transform.GetComponent<Image>().color = new Color(0.91f, 0.91f, 0.91f, 0.404f);
		}

		public void onRouteExit() {
			makeDefaultColorOnRouteTile();
			StopCoroutine(autoRouteCoroutine);
			HistoryService.stopPlayingRoute();
		}
	}
}