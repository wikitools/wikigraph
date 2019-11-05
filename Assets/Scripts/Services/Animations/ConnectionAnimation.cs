using System.Collections;

namespace Services.Animations {
	public class ConnectionAnimation: Animation {
		public AnimationDirection Direction;
		
		public ConnectionAnimation(IEnumerator function, AnimationDirection direction) : base(function) {
			Direction = direction;
		}
	}
}