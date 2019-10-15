using System.Collections.Generic;
using Model.Connection;
using UnityEngine;

namespace Services.Connection {
	public class RouteService {
		private static readonly int CURVE_ANGLE_HEIGHT = 80;
		private static readonly float CURVE_BEND_PROPORTIONS = 0.5f;
		private static int CURVE_SKEW_ANGLE = 20;

		public static Route GenerateRoute(Vector3 from, Vector3 to, Vector3 relSpherePoint) {
			var controlPoints = new List<Vector3>();
			var pointDir = Vector3.ProjectOnPlane(relSpherePoint, Vector3.up);
			controlPoints.Add(pointDir);
			controlPoints.Add(relSpherePoint);
			controlPoints.Add(relSpherePoint + (relSpherePoint - pointDir) * .5f);

			var controlPointArray = controlPoints.ToArray();
			var curveSegments = BezierCurveService.GenerateBezierCurve(controlPointArray);
			return new Route {SegmentPoints = curveSegments, ControlPoints = controlPointArray, SpherePoint = relSpherePoint};
		}
		
		private Route oldAlg(Vector3 from, Vector3 to, Vector3 spherePoint) {
			Vector3 direction = to - from;
			Vector3 normalAxis = Vector3.Cross(direction, Vector3.up);
			normalAxis = Quaternion.AngleAxis(CURVE_SKEW_ANGLE, direction) * normalAxis;
			var rotation = Quaternion.AngleAxis(-CURVE_ANGLE_HEIGHT, normalAxis);
			Vector3 fromControlPointDirection = (1 - CURVE_BEND_PROPORTIONS) * (rotation * direction) + direction;
			Vector3 toControlPointDirection = CURVE_BEND_PROPORTIONS * (rotation * -direction);
			var controlPoints = new[] {Vector3.zero, fromControlPointDirection, toControlPointDirection, to - from};
			var curveSegments = BezierCurveService.GenerateBezierCurve(controlPoints);
			return new Route {SegmentPoints = curveSegments, ControlPoints = controlPoints, SpherePoint = spherePoint};
		}
	}
}