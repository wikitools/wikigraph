using Services.History;
using Services.History.Actions;
using Services.RoutesFiles;
using System;
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
					if (node == null) networkController.SetSelectedNode("");
					else networkController.SetSelectedNode(node.ID.ToString());
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
					StartCoroutine(HistoryService.autoRoutes());
				};
				createRoutesObjects();
			};
		}

		public bool isPlayingRoute() {
			return HistoryService.playsRoute;
		}

		public void createRoutesObjects() {
			int i = 0;
			int[] lengths = HistoryService.getLengths();
			foreach (string name in HistoryService.getNames()) {
				GameObject temp = Instantiate(RouteTemplate, RouteParent.transform);
				string[] getFileName = name.Split('/');
				temp.transform.GetChild(0).GetComponent<Text>().text = getFileName[getFileName.Length - 1].Split('.')[0];
				temp.transform.GetChild(1).GetComponent<Text>().text = "Route Length: <color=black>" + lengths[i].ToString() + "</color>";
				temp.transform.GetChild(2).name = i.ToString();
				temp.transform.GetChild(2).GetComponent<Button>().onClick.AddListener(() => onRouteButtonClicked());
				temp.transform.position = temp.transform.position + new Vector3(0, -64 * i, 0);
				i++;
			}
		}

		public void onRouteButtonClicked() {
			int index = 0;
			if (Int32.TryParse(EventSystem.current.currentSelectedGameObject.name, out index))
				HistoryService.startRoute(index);
		}
	}
}