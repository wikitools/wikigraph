using System.Collections;

namespace Services.Animations {
	public class Animation {
		public static readonly int SEGMENT_CHANGE_SPEED = 4;
		
		public IEnumerator Function;
		public AnimationDirection Direction;
		
		public Animation(IEnumerator function, AnimationDirection direction) {
			Function = function;
			Direction = direction;
		}
	}
}