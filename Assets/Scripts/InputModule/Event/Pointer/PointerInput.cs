using System;
using InputModule.Event.Interfaces;
using UnityEngine;

namespace InputModule.Event.Pointer {
	public abstract class PointerInput: ActivatedInput, InputPoller {
		public PointerActivationType ActivationType;
		public Action<Ray> OnPointed;

		protected abstract Ray GetPointerRay();

		public override void Init() {
			if(!IsButtonActivated) return;
			if (ActivationType == PointerActivationType.Press) {
				ActivationButton.OnPress += () => OnPointed(GetPointerRay());
			} else {
				ActivationButton.OnRelease += () => OnPointed(GetPointerRay());
			}
		}

		public new void CheckForInput() {
			base.CheckForInput();
			if (!IsButtonActivated) {
				OnPointed(GetPointerRay());
			}
		}
	}
	
	public enum PointerActivationType {
		Press = 0, Release = 1
	}
}