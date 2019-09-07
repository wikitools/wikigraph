﻿using Services;
using System;
using UnityEngine;

namespace Controllers {
	public class GraphController : MonoBehaviour {
		public GameObject Infographic; //TODO: move
		private NetworkController networkController;
		public float WorldRadius;

		public static Graph Graph { get; } = new Graph();

		public ObservableProperty<GraphMode> GraphMode = new ObservableProperty<GraphMode>(Controllers.GraphMode.FREE_FLIGHT);
		public ObservableProperty<ConnectionMode> ConnectionMode = new ObservableProperty<ConnectionMode>(Controllers.ConnectionMode.CHILDREN);

		private void Awake() {
			networkController = GetComponent<NetworkController>();
		}

		public void SwitchConnectionMode() {
			SetConnectionMode(ConnectionMode.Value == Controllers.ConnectionMode.PARENTS ? Controllers.ConnectionMode.CHILDREN : Controllers.ConnectionMode.PARENTS);
		}

		public void SetConnectionMode(ConnectionMode connectionMode) {
			if (GraphMode.Value == Controllers.GraphMode.FREE_FLIGHT) return;
			networkController.SetConnectionMode(connectionMode);
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
