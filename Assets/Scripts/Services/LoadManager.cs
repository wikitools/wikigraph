using System.Collections.Generic;
using UnityEngine;
using Animation = Services.Animations.Animation;

namespace Services {
	public abstract class LoadManager<A> where A: Animation {
		protected readonly Dictionary<GameObject, A> ActiveAnimations = new Dictionary<GameObject, A>();
		
	}
}