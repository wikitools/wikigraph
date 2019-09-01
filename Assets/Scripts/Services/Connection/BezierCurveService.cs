using System;
using UnityEngine;

namespace Services.Connection {
	public static class BezierCurveService {
		private static readonly int BEZIER_SEGMENT_NUMBER_MULTIPLIER = 1;

		public static Vector3[] GenerateBezierCurve(Vector3[] controlPoints) {
			int segmentNumber = (int) (ApproximateCurveLength(controlPoints) * BEZIER_SEGMENT_NUMBER_MULTIPLIER);
			var segments = new Vector3[segmentNumber + 1];
			for (var i = 0; i <= segmentNumber; i++)
				segments[i] = CalcBezierCurvePoint(i / (double) segmentNumber, controlPoints);
			return segments;
		}

		private static Vector3 CalcBezierCurvePoint(double t, Vector3[] controlPoints) {
			var loops = controlPoints.Length;
			Vector3 point = Vector3.zero;
			for (int i = 0; i < loops; i++)
				point += (float) Math.Pow(1 - t, loops - i - 1) * (float) Math.Pow(t, i) * controlPoints[i];
			return point;
		}

		private static double ApproximateCurveLength(Vector3[] controlPoints) {
			double chord = Vector3.Distance(controlPoints[0], controlPoints[controlPoints.Length - 1]);
			double controlNet = 0;
			for (int i = 0; i < controlPoints.Length - 1; i++)
				controlNet += Vector3.Distance(controlPoints[i], controlPoints[i + 1]);
			return (chord + controlNet) / 2d;
		}
	}
}