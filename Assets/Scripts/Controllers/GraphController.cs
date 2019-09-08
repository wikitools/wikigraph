﻿using System;
using Services;
using UnityEngine;

namespace Controllers {
	public class GraphController : MonoBehaviour {
		public GameObject Infographic; //TODO: move

		public float WorldRadius;

		public static Graph Graph { get; } = new Graph();

		public ObservableProperty<GraphMode> GraphMode = new ObservableProperty<GraphMode>(Controllers.GraphMode.FREE_FLIGHT);
		public ObservableProperty<ConnectionMode> ConnectionMode = new ObservableProperty<ConnectionMode>(Controllers.ConnectionMode.CHILDREN);

		public void SwitchConnectionMode() {
			SetConnectionMode(ConnectionMode.Value == Controllers.ConnectionMode.PARENTS
				? Controllers.ConnectionMode.CHILDREN
				: Controllers.ConnectionMode.PARENTS);
		}

		public void SetConnectionMode(ConnectionMode connectionMode) {
			if (GraphMode.Value == Controllers.GraphMode.FREE_FLIGHT) return;
			networkController.SetConnectionMode(connectionMode);
		}

		private NetworkController networkController;

		private void Awake() {
			networkController = GetComponent<NetworkController>();
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