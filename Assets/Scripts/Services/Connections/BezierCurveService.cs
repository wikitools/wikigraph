using System;
using System.Linq;
using UnityEngine;

namespace Services.Connection {
	public static class BezierCurveService {
		private static readonly int B_SPLINE_DEGREE = 4;
		
		private static readonly float BEND_PRECISION_MULTIPLIER = 3, STRAIGHT_PRECISION_MULTIPLIER = 1;

		public static Vector3[] GenerateBSpline(Vector3[] controlPoints, int lastPrecisionPointIndex) {
			var knots = CalcKnotPoints(controlPoints.Length);
			
			var bendLength = ApproximateCurveLength(controlPoints, lastPrecisionPointIndex);
			var length = ApproximateCurveLength(controlPoints, controlPoints.Length - 1);
			var bendSegments = (int) (bendLength * BEND_PRECISION_MULTIPLIER);
			var straightSegments = (int) ((length - bendLength) * STRAIGHT_PRECISION_MULTIPLIER);
			int segmentNumber = bendSegments + straightSegments;
			var segments = new Vector3[segmentNumber + 1];
			
			var bendPercent = bendLength / length;
			for (var i = 0; i <= segmentNumber; i++)
				segments[i] = CalcBSplinePoint(Mathf.Min(i, bendSegments) / (float) bendSegments * bendPercent 
				                               + Mathf.Max(0, i - bendSegments) / (float) straightSegments * (1 - bendPercent), controlPoints, knots);
			segments[0] = controlPoints[0];
			return segments;
		}

		private static float[] CalcKnotPoints(int controlPointsLength) {
			var knots = new float[B_SPLINE_DEGREE + controlPointsLength + 1];
			for (int i = B_SPLINE_DEGREE + 1; i < knots.Length; i++) {
				if (i < knots.Length - B_SPLINE_DEGREE - 1)
					knots[i] = i / (float) (knots.Length - 1);
				else
					knots[i] = 1;
			}
			return knots;
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

		private static float ApproximateCurveLength(Vector3[] controlPoints, int index) {
			float length = 0;
			for (int i = 1; i <= index; i++)
				length += Vector3.Distance(controlPoints[i - 1], controlPoints[i]);
			return length;
		}
	}
}