using System.Collections;

namespace Services.Animations {
	public class Animation {
		public IEnumerator Function;
		
		public Animation(IEnumerator function) {
			Function = function;
		}
	}
}