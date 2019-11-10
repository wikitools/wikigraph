using System;
using System.Collections;
using Services;
using UnityEngine;

namespace Controllers {
	public class SkyboxController : MonoBehaviour {

		public GameObject Graph;
		public Material SkyboxMaterial;

		private GraphController graphController;

		void Awake() {
			graphController = Graph.GetComponent<GraphController>();
		}

		private void Start() {
			SkyboxMaterial.SetFloat("_Blend", 0f);
			graphController.GraphMode.OnValueChanged += mode => {
				if (graphController.GraphMode.Value == Controllers.GraphMode.FREE_FLIGHT) {
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
}