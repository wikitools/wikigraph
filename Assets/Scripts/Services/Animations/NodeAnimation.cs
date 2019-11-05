using System.Collections;

namespace Services.Animations {
	public class NodeAnimation: Animation {
		public readonly float Scale;

		public NodeAnimation(IEnumerator function, float scale) : base(function) {
			Scale = scale;
		}
	}
}