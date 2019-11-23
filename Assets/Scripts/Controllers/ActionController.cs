using Model;
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
	public class ActionController : MonoBehaviour {
		private NodeController nodeController;
		public NetworkController networkController { get; private set; }
		private GraphController graphController;
		public ActionService ActionService { get; private set; }
		public RouteController routeController { get; private set; }
	
		public NodeChangedSource nodeChangedSource { get; set; }
		public enum NodeChangedSource {
			User,
			History,
			Route,
			Search
		}

		void Awake() {
			networkController = GetComponent<NetworkController>();
			nodeController = GetComponent<NodeController>();
			graphController = GetComponent<GraphController>();
			routeController = GetComponent<RouteController>();
		}

		private void Start() {
			if (networkController.IsServer()) {
				ActionService = new ActionService();
				nodeChangedSource = NodeChangedSource.User;
				nodeController.OnSelectedNodeChanged += (oldNode, newNode) => {
					if (nodeChangedSource != NodeChangedSource.History) {
						ActionService.RegisterAction(new NodeSelectedAction(oldNode?.ID, newNode?.ID, false));						
					}
					if (nodeChangedSource != NodeChangedSource.Route && routeController.routeService.IsRoutePlaying) networkController.SyncRoutePlaying(false);
					nodeChangedSource = NodeChangedSource.User;
				};
				NodeSelectedAction.selectNodeAction = (node, isRoute) => {
					if (isRoute) nodeChangedSource = NodeChangedSource.Route;
					else nodeChangedSource = NodeChangedSource.History;
					SelectNode(node);
				};
				graphController.ConnectionMode.OnValueChanged += mode => {
					if (nodeChangedSource != NodeChangedSource.History) {
						ActionService.RegisterAction(new ModeChangeAction<ConnectionMode>(mode, false));
					}
					if (nodeChangedSource != NodeChangedSource.Route && routeController.routeService.IsRoutePlaying) networkController.SyncRoutePlaying(false);
					nodeChangedSource = NodeChangedSource.User;
				};
				ModeChangeAction<ConnectionMode>.changeMode = (mode, isRoute) => {
					if (isRoute) nodeChangedSource = NodeChangedSource.Route;
					else nodeChangedSource = NodeChangedSource.History;
					networkController.SetConnectionMode(mode);
				};
			};
		}

		public void SelectNode(uint? index) {
			if (index != null) {
				nodeController.NodeLoadManager.LoadNode(index.Value);
				nodeController.OnNodeLoadSessionEnded?.Invoke();
			}
			networkController.SetSelectedNode(index.ToString());
		}
	}
}
