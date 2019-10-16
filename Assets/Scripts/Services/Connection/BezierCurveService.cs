using System;
using System.Linq;
using UnityEngine;

namespace Services.Connection {
	public static class BezierCurveService {
		private static readonly int B_SPLINE_DEGREE = 4;
		
		private static readonly double BEZIER_SEGMENT_NUMBER_MULTIPLIER = .5;

		public static Vector3[] GenerateBSpline(Vector3[] controlPoints) {
			var knots = new float[B_SPLINE_DEGREE + controlPoints.Length + 1];
			for (int i = B_SPLINE_DEGREE + 1; i < knots.Length; i++) {
				if (i < knots.Length - B_SPLINE_DEGREE - 1)
					knots[i] = i / (float) (knots.Length - 1);
				else
					knots[i] = 1;
			}
			int segmentNumber = (int) (ApproximateCurveLength(controlPoints) * BEZIER_SEGMENT_NUMBER_MULTIPLIER);
			var segments = new Vector3[segmentNumber + 1];
			for (var i = 0; i <= segmentNumber; i++)
				segments[i] = CalcBSplinePoint(i / (float) segmentNumber, controlPoints, knots);
			segments[0] = controlPoints[0];
			return segments;
		}

		private static Vector3 CalcBSplinePoint(float x, Vector3[] controlPoints, float[] knots) {
			int k = 0;
			int p = B_SPLINE_DEGREE;
			Func<int, float> knotVal = ind => knots[Utils.Mod(ind, knots.Length)];
			for (var i = 1; i < knots.Length; i++) {
				if(x <= knots[i])
					break;
				k++;
			}
			var d = Enumerable.Range(0, p + 1).Select(j => controlPoints[Utils.Mod(j + k - p, controlPoints.Length)]).ToArray();
			float alpha;
			for (int r = 1; r <= p; r++) {
				for (int j = p; j >= r; j--) {
					alpha = (x - knotVal(j + k - p)) / (knotVal(j + 1 + k - r) - knotVal(j + k - p));
					d[j] = (1f - alpha) * d[j - 1] + alpha * d[j];
				}
			}
			return d[p];
		}

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