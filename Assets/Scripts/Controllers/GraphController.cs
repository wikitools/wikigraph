using System;
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

		private void Start() {
			HistoryController.startLoading += () => CreateRouteObjects(); 
		}


		private void CreateRouteObjects() {
			string[] names = getRoutesNames();
			int i = 1;
			foreach(string name in names) {
				GameObject temp = Instantiate(RouteTemplate);
				temp.transform.parent = RoutesUI.transform;
				if (i%2 == 0) temp.transform.position = new Vector3(temp.transform.position.x + 4 * i * 2 * -1, temp.transform.position.y, temp.transform.position.z);
				else temp.transform.position = new Vector3(temp.transform.position.x + 4 * i * 2, temp.transform.position.y, temp.transform.position.z);
				temp.GetComponentInChildren<Text>().text = name;
				//var routeImage = RouteTemplate.GetComponentInChildren<Image>();
				//routeImage.sprite = 
				temp.name = "Route" + i.ToString();
			}
		}

		private void Start() {
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