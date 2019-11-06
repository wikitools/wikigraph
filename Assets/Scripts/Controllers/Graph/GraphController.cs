using System;
using System.Collections;
using Services;
using UnityEngine;

namespace Controllers {
	public class GraphController : MonoBehaviour {
		public GameObject Infographic; //TODO: move

		public float WorldRadius;
		public Material SkyboxMaterial;

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

		private void Awake() {
			networkController = GetComponent<NetworkController>();
		}

		private void Start() {
			GraphMode.OnValueChanged += mode => {
				if (GraphMode.Value == Controllers.GraphMode.FREE_FLIGHT)
					ConnectionMode.Value = Controllers.ConnectionMode.CHILDREN;
			};
			GraphMode.OnValueChanged += mode => {
				if (GraphMode.Value == Controllers.GraphMode.FREE_FLIGHT) {
					StopAllCoroutines();
					StartCoroutine(ChangeBlend(SkyboxMaterial.GetFloat("_Blend"), 0f, 1f));
				} else {
					StopAllCoroutines();
					StartCoroutine(ChangeBlend(SkyboxMaterial.GetFloat("_Blend"), 1f, 1f));
				}
			};
		}

		IEnumerator ChangeBlend(float start, float end, float duration) {
			float elapsed = 0.0f;
			while (elapsed < duration) {
				SkyboxMaterial.SetFloat("_Blend", Mathf.Lerp(start, end, elapsed / duration));
				elapsed += Time.deltaTime;
				yield return null;
			}
			SkyboxMaterial.SetFloat("_Blend", end);
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