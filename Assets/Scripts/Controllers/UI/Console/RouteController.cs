﻿using System;
using System.Collections;
using System.IO;
using Services.History;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Controllers.UI.Console {
	public class RouteController : MonoBehaviour {

		private NodeController nodeController;
		private NetworkController networkController;
		private ActionController historyController;

		public RouteService routeService;
		public Action<bool> OnRoutePlayStateChanged;
		IEnumerator autoRouteCoroutine;

		public GameObject RouteTemplate;
		public GameObject RouteParent;
		GameObject[] routesTiles;

		public int secondsToChangeRoute = 8;
		int routeIndex = -1;
		string routesPath;
		string ROUTES_DIR = "Routes";


		void Awake() {
			networkController = GetComponent<NetworkController>();
			nodeController = GetComponent<NodeController>();
			historyController = GetComponent<ActionController>();
		}

		void Start() {
			if (networkController.IsServer()) {
				routesPath = Path.Combine(nodeController.NodeLoadManager.NodeLoader.fileReader.GetDataPackDirectory(), ROUTES_DIR);
				routeService = new RouteService(historyController, secondsToChangeRoute, routesPath);
				OnRoutePlayStateChanged += isStarted => {
					if(isStarted) {
						routeService.IsRoutePlaying = true;
						historyController.nodeChangedSource = ActionController.NodeChangedSource.Route;
						autoRouteCoroutine = routeService.autoRoutes();
						StartCoroutine(autoRouteCoroutine);
					} else {
						makeDefaultColorOnRouteTile();
						StopCoroutine(autoRouteCoroutine);
						routeService.IsRoutePlaying = false;
						routeIndex = -1;
					}
				};
				createRoutesObjects();
			}
		}

		private void OnDestroy() {
			routeService.routesLoader.Dispose();
		}




		public void createRoutesObjects() {
			int i = 0;
			int[] lengths = routeService.routesLoader.routeReader.lengthOfRoutes();
			routesTiles = new GameObject[lengths.Length];
			foreach (string name in routeService.routesLoader.routeReader.namesOfRoutes()) {
				routesTiles[i] = Instantiate(RouteTemplate, RouteParent.transform);
				string getFileName = Path.GetFileNameWithoutExtension(name);
				routesTiles[i].transform.GetChild(0).GetComponent<Text>().text = getFileName;
				routesTiles[i].transform.GetChild(1).GetComponent<Text>().text = "Route Length: <color=black>" + lengths[i] + "</color>";
				routesTiles[i].name = i.ToString();
				routesTiles[i].GetComponent<Button>().onClick.AddListener(() => onRouteButtonClicked());
				routesTiles[i].transform.position = routesTiles[i].transform.position + new Vector3(0, -64 * i, 0);
				i++;
			}
		}

		public void onRouteButtonClicked() {
			
			int newIndex;
			if (Int32.TryParse(EventSystem.current.currentSelectedGameObject.name, out newIndex)) {
				if (newIndex != routeIndex) {
					if (routeService.IsRoutePlaying) networkController.SyncRoutePlaying(false);
					routeIndex = newIndex;
					routeService.startRoute(routeIndex);
					routesTiles[routeIndex].transform.GetComponent<Image>().color = new Color(0.341f, 0.58f, 0.808f, 0.65f);
					routesTiles[routeIndex].transform.GetChild(2).GetComponent<Text>().text = "Stop";
					networkController.ToggleConsole();
				}
				else {
					if (routeService.IsRoutePlaying) networkController.SyncRoutePlaying(false);
				}

			} else {
				if (routeService.IsRoutePlaying) networkController.SyncRoutePlaying(false);
			}
		}

		public void makeDefaultColorOnRouteTile() {
			routesTiles[routeIndex].transform.GetComponent<Image>().color = new Color(0.91f, 0.91f, 0.91f, 0.404f);
			routesTiles[routeIndex].transform.GetChild(2).GetComponent<Text>().text = "Start";
		}
	}

}
