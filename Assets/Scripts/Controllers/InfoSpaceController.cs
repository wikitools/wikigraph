using System.Collections;
using System.Collections.Generic;
using Inspector;
using UnityEngine;
using UnityEngine.UI;
using Model;

namespace Controllers {
	public class InfoSpaceController : MonoBehaviour {

		public GameObject Entity;
		public GameObject Graph;
		public GameObject Header;
		public GameObject Grid;
		public bool State;

		private CanvasGroup[] BackgroundCanvasGroups;
		private float SpaceOpacity;

		private InputController inputController;
		private NodeController nodeController;
		private NetworkController networkController;
		private ConnectionController connectionController;

		void Awake() {
			networkController = Graph.GetComponent<NetworkController>();
			inputController = Graph.GetComponent<InputController>();
			nodeController = Graph.GetComponent<NodeController>();
			connectionController = Graph.GetComponent<ConnectionController>();
		}

		void Start() {
			BackgroundCanvasGroups = transform.GetComponentsInChildren<CanvasGroup>();
			SetRendererSortingOrder(transform, 20);
			State = true;
			Header.SetActive(!State);
			SpaceOpacity = State ? 1.0f : 0.0f;
		}

		private void SetRendererSortingOrder(Transform obj, int order) {
			obj.GetComponent<MeshRenderer>().sortingOrder = order;
		}

		public void ToggleVisibility() {
			if (!networkController.IsServer())
				return;
			if (State && SpaceOpacity == 1.0f) {
				StopAllCoroutines();
				StartCoroutine(ChangeOpacity(SpaceOpacity, 0f, 1f));
				inputController.SetBlockInput(false, InputBlockType.INFO_SPACE);
			}
			if (!State && SpaceOpacity == 0.0f) {
				StopAllCoroutines();
				StartCoroutine(ChangeOpacity(SpaceOpacity, 1f, 1f));
				inputController.SetBlockInput(true, InputBlockType.INFO_SPACE);
			}
		}

		IEnumerator ChangeOpacity(float start, float end, float duration) {
			float elapsed = 0.0f;
			if (!State) {
				Header.SetActive(false);
				SetRendererSortingOrder(Grid.transform, 50);
			}
			while (elapsed < duration) {
				SpaceOpacity = Mathf.Lerp(start, end, elapsed / duration);
				transform.GetComponent<MeshRenderer>().material.color = new Color(0f, 0f, 0f, SpaceOpacity);
				foreach (CanvasGroup canvas in BackgroundCanvasGroups)
					canvas.alpha = SpaceOpacity;
				elapsed += Time.deltaTime;
				yield return null;
			}
			transform.GetComponent<MeshRenderer>().material.color = new Color(0f, 0f, 0f, end);
			foreach (CanvasGroup canvas in BackgroundCanvasGroups)
				canvas.alpha = end;
			SpaceOpacity = end;
			if (State) { 
				Header.SetActive(true);
				SetRendererSortingOrder(Grid.transform, -10);
			}
			State = !State;
		}

		void Update() {
			transform.position = Entity.transform.position;
		}

	}
}
