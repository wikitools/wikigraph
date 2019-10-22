using System;
using System.Collections.Generic;
using System.Linq;
using Model.Connection;
using UnityEngine;

namespace Services.Connection {
	public class RouteService {
		private static readonly int CURVE_BASE_ANGLE = 30;
		private static int CURVE_SKEW_ANGLE = 10;
		
		private static readonly int CURVE_ANGLE_HEIGHT = 80;
		private static readonly float CURVE_BEND_PROPORTIONS = 0.5f;

		public static Route GenerateRoute(Vector3 from, Vector3 to, Vector3 relSpherePoint) {
			var controlPoints = new List<Vector3> {Vector3.zero};
			var pointDir = Vector3.ProjectOnPlane(relSpherePoint, Vector3.up);
			controlPoints.Add(pointDir);
			controlPoints.Add(relSpherePoint);
			controlPoints.Add(relSpherePoint + (relSpherePoint - pointDir) * .5f);
			var normalAxis = Vector3.Cross(controlPoints[1], Vector3.up);
			Func<float, Quaternion> rotate = angle => Quaternion.AngleAxis(angle, normalAxis);
			var skewRotation = Quaternion.AngleAxis(CURVE_SKEW_ANGLE, Vector3.up);
			
			var toNodeVector = to - (from + controlPoints.Last());
			var sphereSegmentLen = (controlPoints[3] - controlPoints[1]).magnitude;
			controlPoints.Add(controlPoints.Last() + skewRotation * rotate(CURVE_BASE_ANGLE) * toNodeVector.normalized * sphereSegmentLen);
			
			toNodeVector = to - (from + controlPoints.Last());
			controlPoints.Add(controlPoints.Last() + Quaternion.Inverse(skewRotation * rotate(CURVE_BASE_ANGLE / 2)) * toNodeVector / 2f);
			controlPoints.Add(to - from);

			var controlPointArray = controlPoints.ToArray();
			var curveSegments = BezierCurveService.GenerateBSpline(controlPointArray, 4);
			return new Route {SegmentPoints = curveSegments, ControlPoints = controlPointArray, SpherePoint = relSpherePoint};
		}
	}
}