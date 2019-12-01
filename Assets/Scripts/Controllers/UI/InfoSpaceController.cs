using System;
using System.Collections;
using Inspector;
using UnityEngine;

namespace Controllers.UI {
	public class InfoSpaceController : MonoBehaviour {

		public GameObject Entity;
		public GameObject Graph;
		public GameObject Header;
		public GameObject Grid;
		public bool State;

		public static Action playInfoSound;
		private CanvasGroup[] BackgroundCanvasGroups;
		private float SpaceOpacity;

		private InputController inputController;
		private NodeController nodeController;
		private HeaderController headerController;

		void Awake() {
			inputController = Graph.GetComponent<InputController>();
			nodeController = Graph.GetComponent<NodeController>();
			headerController = Header.GetComponent<HeaderController>();
		}

		void Start() {
			BackgroundCanvasGroups = transform.GetComponentsInChildren<CanvasGroup>();
			SetRendererSortingOrder(transform, 20);
			State = true;
			headerController.SetEnabled(!State);
			SpaceOpacity = State ? 1.0f : 0.0f;
		}

		private void SetRendererSortingOrder(Transform obj, int order) {
			obj.GetComponent<MeshRenderer>().sortingOrder = order;
		}

		public void ToggleVisibility() {
			if (State && SpaceOpacity == 1.0f) {
				StopAllCoroutines();
				StartCoroutine(ChangeOpacity(SpaceOpacity, 0f, 1f));
				playInfoSound?.Invoke();
			}
			if (!State && SpaceOpacity == 0.0f) {
				StopAllCoroutines();
				nodeController.HighlightedNode = null;
				StartCoroutine(ChangeOpacity(SpaceOpacity, 1f, 1f));
				playInfoSound?.Invoke();
			}
		}

		IEnumerator ChangeOpacity(float start, float end, float duration) {
			float elapsed = 0.0f;
			if (!State) {
				headerController.SetEnabled(false);
				SetRendererSortingOrder(Grid.transform, 50);
				inputController.SetBlockInput(true, InputBlockType.INFO_SPACE);
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
				headerController.SetEnabled(true);
				SetRendererSortingOrder(Grid.transform, -10);
				inputController.SetBlockInput(false, InputBlockType.INFO_SPACE);
			}
			State = !State;
		}

		void Update() {
			transform.position = Entity.transform.position;
		}

	}
}
