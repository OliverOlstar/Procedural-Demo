using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OliverLoescher.Util
{
    public static class Math
    {
		public const float NEARZERO = 0.0001f;

		public static Vector3 Horizontal(this Vector3 pVector) { pVector.y = 0; return pVector; }
		public static Vector2 Horizontal2D(this Vector3 pVector) => new Vector2(pVector.x, pVector.z);
		public static Vector3 Horizontal(this Vector3 pVector, Vector3 pUp) => Vector3.ProjectOnPlane(pVector, pUp);

		public static Vector3 Horizontalize(this Vector3 pVector) => new Vector3(pVector.x, 0.0f, pVector.z).normalized;
		public static Vector3 Horizontalize(this Vector3 pVector, float pMagnitude) => new Vector3(pVector.x, 0.0f, pVector.z).normalized * pMagnitude;

		public static Vector3 Horizontalize(this Vector3 pVector, Vector3 pUp) => Vector3.ProjectOnPlane(pVector, pUp).normalized;
		public static Vector3 Horizontalize(this Vector3 pVector, Vector3 pUp, float pMagnitude) => Vector3.ProjectOnPlane(pVector, pUp).normalized * pMagnitude;

		public static Vector3 Inverse(in Vector3 pVector) => new Vector3(1.0f / pVector.x, 1.0f / pVector.y, 1.0f / pVector.z);

		public static float Clamp(this float pValue, Vector2 pClamp) => Mathf.Clamp(pValue, pClamp.x, pClamp.y);

		public static Vector3 Mult(Vector3 pA, Vector3 pB) => new Vector3(pA.x * pB.x, pA.y * pB.y, pA.z * pB.z);
		public static Vector3 Add(Vector3 pA, Vector3 pB) => new Vector3(pA.x + pB.x, pA.y + pB.y, pA.z + pB.z);
		public static Vector3 Add(params Vector3[] pVectors)
		{
			Vector3 vector = Vector3.zero;
			foreach (Vector3 v in pVectors)
			{
				vector = Add(vector, v);
			}
			return vector;
		}
		public static Vector3 Add(IEnumerable<Vector3> pVectors)
		{
			Vector3 vector = Vector3.zero;
			foreach (Vector3 v in pVectors)
			{
				vector = Add(vector, v);
			}
			return vector;
		}

		public static float AddPercents(IEnumerable<float> pValues)
		{
			float value = 1;
			foreach (float v in pValues)
			{
				if (v > 0)
				{
					value += v;
				}
			}
			foreach (float v in pValues)
			{
				if (v < 0)
				{
					value *= Mathf.Clamp01(1 - Mathf.Abs(v));
				}
			}
			return value;
		}

		public static Quaternion Difference(this Quaternion to, Quaternion from) => to * Quaternion.Inverse(from);
		public static Quaternion Add(this Quaternion start, Quaternion diff) => diff * start;
		public static Quaternion UpForwardRotation(Vector3 forward, Vector3 up) => Quaternion.FromToRotation(Vector3.up, up) * Quaternion.LookRotation(forward);

		#region Lerp
		public static float LerpUnclamped(float pA, float pB, float pC, float pT)
		{
			pT *= 2.0f;
			if (pT <= 1.0f)
			{
				return Mathf.LerpUnclamped(pA, pB, pT);
			}
			return Mathf.LerpUnclamped(pB, pC, pT - 1);
		}
		public static Vector2 LerpUnclamped(Vector2 pA, Vector2 pB, Vector2 pC, float pT)
		{
			pT *= 2.0f;
			if (pT <= 1.0f)
			{
				return Vector2.LerpUnclamped(pA, pB, pT);
			}
			return Vector2.LerpUnclamped(pB, pC, pT - 1);
		}
		public static Vector3 LerpUnclamped(Vector3 pA, Vector3 pB, Vector3 pC, float pT)
		{
			pT *= 2.0f;
			if (pT <= 1.0f)
			{
				return Vector3.LerpUnclamped(pA, pB, pT);
			}
			return Vector3.LerpUnclamped(pB, pC, pT - 1);
		}
		#endregion Lerp

		#region Compare
		public static bool DistanceEqual(this Vector3 pA, Vector3 pB, float pDistance) => (pA - pB).sqrMagnitude == Mathf.Pow(pDistance, 2);
		public static bool DistanceGreaterThan(this Vector3 pA, Vector3 pB, float pDistance) => (pA - pB).sqrMagnitude > Mathf.Pow(pDistance, 2);
		public static bool DistanceEqualGreaterThan(this Vector3 pA, Vector3 pB, float pDistance) => (pA - pB).sqrMagnitude >= Mathf.Pow(pDistance, 2);
		public static bool DistanceLessThan(this Vector3 pA, Vector3 pB, float pDistance) => (pA - pB).sqrMagnitude < Mathf.Pow(pDistance, 2);
		public static bool DistanceEqualLessThan(this Vector3 pA, Vector3 pB, float pDistance) => (pA - pB).sqrMagnitude <= Mathf.Pow(pDistance, 2);

		public static bool DistanceOnPlaneEqual(this Vector3 pA, Vector3 pB, float pDistance, Vector3 pPlaneNormal) =>
			Vector3.ProjectOnPlane(pA - pB, pPlaneNormal).sqrMagnitude == Mathf.Pow(pDistance, 2);
		public static bool DistanceOnPlaneGreaterThan(this Vector3 pA, Vector3 pB, float pDistance, Vector3 pPlaneNormal) =>
			Vector3.ProjectOnPlane(pA - pB, pPlaneNormal).sqrMagnitude > Mathf.Pow(pDistance, 2);
		public static bool DistanceOnPlaneEqualGreaterThan(this Vector3 pA, Vector3 pB, float pDistance, Vector3 pPlaneNormal) =>
			Vector3.ProjectOnPlane(pA - pB, pPlaneNormal).sqrMagnitude >= Mathf.Pow(pDistance, 2);
		public static bool DistanceOnPlaneLessThan(this Vector3 pA, Vector3 pB, float pDistance, Vector3 pPlaneNormal) =>
			Vector3.ProjectOnPlane(pA - pB, pPlaneNormal).sqrMagnitude < Mathf.Pow(pDistance, 2);
		public static bool DistanceOnPlaneEqualLessThan(this Vector3 pA, Vector3 pB, float pDistance, Vector3 pPlaneNormal) =>
			Vector3.ProjectOnPlane(pA - pB, pPlaneNormal).sqrMagnitude <= Mathf.Pow(pDistance, 2);

		public static bool DistanceHorizontalEqual(this Vector3 pA, Vector3 pB, float pDistance) =>
			Horizontal2D(pA - pB).sqrMagnitude == Mathf.Pow(pDistance, 2);
		public static bool DistanceHorizontalGreaterThan(this Vector3 pA, Vector3 pB, float pDistance) =>
			Horizontal2D(pA - pB).sqrMagnitude > Mathf.Pow(pDistance, 2);
		public static bool DistanceHorizontalEqualGreaterThan(this Vector3 pA, Vector3 pB, float pDistance) =>
			Horizontal2D(pA - pB).sqrMagnitude >= Mathf.Pow(pDistance, 2);
		public static bool DistanceHorizontalLessThan(this Vector3 pA, Vector3 pB, float pDistance) =>
			Horizontal2D(pA - pB).sqrMagnitude < Mathf.Pow(pDistance, 2);
		public static bool DistanceHorizontalEqualLessThan(this Vector3 pA, Vector3 pB, float pDistance) =>
			Horizontal2D(pA - pB).sqrMagnitude <= Mathf.Pow(pDistance, 2);

		public static float DistanceHorizontal(Vector3 pA, Vector3 pB) => Horizontal2D(pA - pB).magnitude;
		#endregion Compare
	}
}