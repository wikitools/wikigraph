﻿using System;
using Services;
using UnityEngine;
using UnityEngine.UI;

namespace Controllers {
	public class GraphController : MonoBehaviour {
		public GameObject Infographic; //TODO: move
		public GameObject RouteTemplate;
		public GameObject RoutesUI;

		public float WorldRadius;

		public static Graph Graph { get; } = new Graph();

		public ObservableProperty<GraphMode> GraphMode = new ObservableProperty<GraphMode>(Controllers.GraphMode.FREE_FLIGHT);
		public ObservableProperty<ConnectionMode> ConnectionMode = new ObservableProperty<ConnectionMode>(Controllers.ConnectionMode.CHILDREN);

		public ConnectionMode GetSwitchedConnectionMode() {
			return ConnectionMode.Value == Controllers.ConnectionMode.PARENTS
				? Controllers.ConnectionMode.CHILDREN : Controllers.ConnectionMode.PARENTS;
		}

		private void SetConnectionMode(ConnectionMode connectionMode) {
			if (GraphMode.Value == Controllers.GraphMode.FREE_FLIGHT) return;
			networkController.SetConnectionMode(connectionMode);
		}

		public void SwitchConnectionMode() {
			SetConnectionMode(GetSwitchedConnectionMode());
		}

		private NetworkController networkController;
		private NodeController nodeController;
		private HistoryController historyController;
		public static Func<string[]> getRoutesNames;

		private void Awake() {
			networkController = GetComponent<NetworkController>();
			nodeController = GetComponent<NodeController>();
			historyController = GetComponent<HistoryController>();
		}

		private void CreateRouteObjects() {
			string[] names = getRoutesNames();
			int i = 0;
			foreach(string name in names) {
				GameObject temp = Instantiate(RouteTemplate);
				temp.transform.parent = RoutesUI.transform;
				if(i%2 == 0) temp.transform.position = new Vector3((temp.transform.position.x + 4 * (int)(i/2) + 4) , temp.transform.position.y, 8); //todo z
				else temp.transform.position = new Vector3(((temp.transform.position.x + 4 * (int)(i/2) + 4)*-1 ), temp.transform.position.y, 8); //todo z
				var tmp = name.Split('/');
				var tmp2 = tmp[tmp.Length-1].Split('.');
				temp.GetComponentInChildren<Text>().text = tmp2[0];
				//var routeImage = RouteTemplate.GetComponentInChildren<Image>();
				//routeImage.sprite = 
				temp.name = "Route" + i.ToString();
				i++;
			}
		}

		private void Start() {
			HistoryController.startLoading += () => CreateRouteObjects(); 
			GraphMode.OnValueChanged += mode => {
				if (GraphMode.Value == Controllers.GraphMode.FREE_FLIGHT)
					ConnectionMode.Value = Controllers.ConnectionMode.CHILDREN;
			};
		}
	}

	public class ObservableProperty<T> {
		private T value;

		public T Value {
			get { return value; }
			set {
				if (value.Equals(this.value))
					return;
				this.value = value;
				OnValueChanged?.Invoke(value);
			}
		}

		public Action<T> OnValueChanged;

		public ObservableProperty(T value) {
			Value = value;
		}
	}

	public enum GraphMode {
		FREE_FLIGHT,
		NODE_TRAVERSE
	}

	public enum ConnectionMode {
		PARENTS,
		CHILDREN
	}
}