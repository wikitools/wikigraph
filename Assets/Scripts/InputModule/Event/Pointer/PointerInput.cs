using System;
using InputModule.Event.Interfaces;
using UnityEngine;

namespace InputModule.Event.Pointer {
	public abstract class PointerInput: ActivatedInput, InputPoller {
		public PointerActivationType ActivationType;
		public Action<Ray> OnPointed;

		protected abstract Ray GetPointerRay();

		public override void Init() {
			Action onActivation = ActivationType == PointerActivationType.Press ? ActivationButton.OnPress : ActivationButton.OnRelease;
			onActivation += () => OnPointed(GetPointerRay());
		}

		public new void CheckForInput() {
			base.CheckForInput();
		}
	}
	
	public enum PointerActivationType {
		Press = 0, Release = 1
	}
}