using UnityEngine;

namespace Services.Connection {
	public class ConnectionService {
		private static readonly int CURVE_ANGLE_HEIGHT = 80;
		private static readonly float CURVE_BEND_PROPORTIONS = 0.5f;
		
		public static Model.Connection GenerateConnection(Vector3 from, Vector3 to) {
			Vector3 direction = to - from;
			Vector3 normalAxis = Vector3.Cross(direction, Vector3.up);
			var rotation = Quaternion.AngleAxis(-CURVE_ANGLE_HEIGHT, normalAxis);
			Vector3 fromControlPointDirection = (1 - CURVE_BEND_PROPORTIONS) * (rotation * direction) + direction;
			Vector3 toControlPointDirection = CURVE_BEND_PROPORTIONS * (rotation * -direction);
			var controlPoints = new[] {Vector3.zero, fromControlPointDirection, toControlPointDirection, to - from};
			var curveSegments = BezierCurveService.GenerateBezierCurve(controlPoints);
			return new Model.Connection {SegmentPoints = curveSegments, ControlPoints = controlPoints};
		}
	}
}