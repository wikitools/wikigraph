using System.Collections;
using UnityEngine;

namespace DefaultNamespace {
	public class ConnectionAnimation {
		public static readonly int SEGMENT_CHANGE_SPEED = 2;
		
		public IEnumerator Function;
		public AnimationDirection Direction;
		
		public ConnectionAnimation(IEnumerator function, AnimationDirection direction) {
			Function = function;
			Direction = direction;
		}
	}
}