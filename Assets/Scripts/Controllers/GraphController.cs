using System;
using Services;
using UnityEngine;

namespace Controllers {
	public class GraphController : MonoBehaviour {
		public GameObject Infographic; //TODO: move
		
		public float WorldRadius;
		
		public static Graph Graph { get; } = new Graph();
		
		private NodeController nodeController;
		
		public ObservableProperty<GraphMode> GraphMode = new ObservableProperty<GraphMode>(Controllers.GraphMode.FREE_FLIGHT);

		public ObservableProperty<ConnectionMode> ConnectionMode = new ObservableProperty<ConnectionMode>(Controllers.ConnectionMode.CHILDREN);
		
		void Awake() {
			nodeController = GetComponent<NodeController>();
		}
	}

	public class ObservableProperty<T> {
		private T value;
		public T Value {
			get { return value; }
			set {
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
		FREE_FLIGHT, NODE_TRAVERSE
	}

	public enum ConnectionMode {
		PARENTS, CHILDREN
	}
}
