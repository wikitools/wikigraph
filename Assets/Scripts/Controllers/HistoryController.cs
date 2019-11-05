using Model;
using Services.History;
using Services.History.Actions;
using Services.RoutesFiles;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Controllers {
	public class HistoryController : MonoBehaviour {
		private NodeController nodeController;
		private NetworkController networkController;
		private GraphController graphController;
		public HistoryService HistoryService { get; private set; }
		public static Action startLoading;

		public GameObject RouteTemplate;
		public GameObject RoutesUI;

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
					networkController.SetSelectedNode(node.ID.ToString());
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
<<<<<<< HEAD
					return nodeController.NodeLoadManager.LoadNode(id);
=======
					return nodeController.NodeLoadManager.LoadNode(id); 
>>>>>>> 47a07382c82592e3c7672c291ff77daa00c83d57
				};


				CreateRouteObjects();

			};
		}

		private void CreateRouteObjects() {
			string[] names = HistoryService.getNames();
			int i = 0;
			foreach (string name in names) {
				GameObject temp = Instantiate(RouteTemplate);
				temp.transform.parent = RoutesUI.transform;
				if (i % 2 == 0) temp.transform.position = new Vector3((temp.transform.position.x + 4 * (int)(i / 2) + 4), temp.transform.position.y, 8); //todo z
				else temp.transform.position = new Vector3(((temp.transform.position.x + 4 * (int)(i / 2) + 4) * -1), temp.transform.position.y, 8); //todo z
				var tmp = name.Split('/');
				var tmp2 = tmp[tmp.Length - 1].Split('.');
				temp.GetComponentInChildren<Text>().text = tmp2[0];
				//var routeImage = RouteTemplate.GetComponentInChildren<Image>();
				//routeImage.sprite = 
				temp.name = "Route" + i.ToString();
				i++;
			}
		}
	}
}