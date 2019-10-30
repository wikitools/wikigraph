using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animation = Services.Animations.Animation;

namespace Services {
	public abstract class AnimationManager<A> where A: Animation {
		protected readonly Dictionary<GameObject, A> ActiveAnimations = new Dictionary<GameObject, A>();

		private Action<GameObject, A> animationEndAction;
		private readonly MonoBehaviour controller;

		protected AnimationManager(MonoBehaviour controller) {
			this.controller = controller;
		}

		protected void StartAnimation(GameObject key, A animation) {
			if (ActiveAnimations.ContainsKey(key)) {
				var previousAnimation = ActiveAnimations[key];
				controller.StopCoroutine(previousAnimation.Function);
				animationEndAction(key, previousAnimation);
			}
			ActiveAnimations.Add(key, animation);
			controller.StartCoroutine(animation.Function);
		}

		public void SetAnimationEndAction(Action<GameObject, A> animationEndAction) {
			this.animationEndAction = animationEndAction;
		}
	}
}