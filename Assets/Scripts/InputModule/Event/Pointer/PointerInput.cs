using System;
using InputModule.Event.Interfaces;
using UnityEngine;

namespace InputModule.Event.Pointer {
	public abstract class PointerInput : ActivatedInput, InputPoller {
		public PointerActivationType ActivationType;
		public Action<Ray> OnPointed;

		protected abstract Ray GetPointerRay();

		public override void Init() {
			base.Init();
			if (!IsButtonActivated) return;
			if (ActivationType == PointerActivationType.Press) {
				ActivationButton.OnPress += InvokeOnPointed;
			} else {
				ActivationButton.OnRelease += InvokeOnPointed;
			}
		}

		public override void CheckForInput() {
			base.CheckForInput();
			if (!IsButtonActivated)
				InvokeOnPointed();
		}

		private void InvokeOnPointed() {
			if(!Blocked)
				OnPointed?.Invoke(GetPointerRay());
		}
	}

	public enum PointerActivationType {
		Press = 0,
		Release = 1
	}
}