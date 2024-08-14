using System.Collections.Generic;
using UnityEngine;

namespace ODev.Util
{
	public static class Math
	{
		public const float NEARZERO = 0.0001f;

		public static Vector3 Horizontal(this Vector3 pVector) { pVector.y = 0; return pVector; }
		public static Vector2 Horizontal2D(this Vector3 pVector) => new(pVector.x, pVector.z);
		public static Vector3 Horizontal(this Vector3 pVector, Vector3 pUp) => Vector3.ProjectOnPlane(pVector, pUp);

		public static Vector3 Horizontalize(this Vector3 pVector) => new Vector3(pVector.x, 0.0f, pVector.z).normalized;
		public static Vector3 Horizontalize(this Vector3 pVector, float pMagnitude) => new Vector3(pVector.x, 0.0f, pVector.z).normalized * pMagnitude;

		public static Vector3 Horizontalize(this Vector3 pVector, Vector3 pUp) => Vector3.ProjectOnPlane(pVector, pUp).normalized;
		public static Vector3 Horizontalize(this Vector3 pVector, Vector3 pUp, float pMagnitude) => Vector3.ProjectOnPlane(pVector, pUp).normalized * pMagnitude;

		public static Vector3 ProjectOnPlane(this Vector3 pVector, Vector3 pUp) => Vector3.ProjectOnPlane(pVector, pUp);

		public static Vector3 Left(this Vector3 pVector, Vector3 pAxis) => Vector3.Cross(pVector, pAxis);
		public static Vector3 Right(this Vector3 pVector, Vector3 pAxis) => -Vector3.Cross(pVector, pAxis);

		public static Vector3 Inverse(in Vector3 pVector) => new(1.0f / pVector.x, 1.0f / pVector.y, 1.0f / pVector.z);
		public static Vector3 Inverse(this Vector3 pVector) => pVector *= -1.0f;

		public static float Clamp(this float pValue, Vector2 pClamp) => Mathf.Clamp(pValue, pClamp.x, pClamp.y);
		public static float ClampMax(this float pValue, float pMax) => Mathf.Min(pValue, pMax);
		public static float ClampMin(this float pValue, float pMin) => Mathf.Min(pValue, pMin);
		public static float Clamp01(this float pValue) => Mathf.Clamp01(pValue);
		public static float Loop(this float pValue, float pMax)
		{
			while (pValue >= pMax)
			{
				pValue -= pMax;
			}
			while (pValue < 0)
			{
				pValue += pMax;
			}
			return pValue;
		}
		public static int Clamp(this int pValue, Vector2 pClamp) => Mathf.Clamp(pValue, (int)pClamp.x, (int)pClamp.y);
		public static int Clamp(this int pValue, Vector2Int pClamp) => Mathf.Clamp(pValue, pClamp.x, pClamp.y);
		public static int ClampMax(this int pValue, int pMax) => Mathf.Min(pValue, pMax);
		public static int ClampMin(this int pValue, int pMin) => Mathf.Min(pValue, pMin);
		/// <summary>Wraps value to max, returned value will always be below the max</summary>
		/// <param name="pMax">Max is exclusive</param>
		public static int Loop(this int pValue, int pMax)
		{
			while (pValue >= pMax)
			{
				pValue -= pMax;
			}
			while (pValue < 0)
			{
				pValue += pMax;
			}
			return pValue;
		}

		public static float DistanceXZ(Vector3 pA, Vector3 pB) => Horizontal2D(pA - pB).magnitude;

		public static Vector3 Mult(Vector3 pA, Vector3 pB) => new(pA.x * pB.x, pA.y * pB.y, pA.z * pB.z);
		public static Vector3 Add(Vector3 pA, Vector3 pB) => new(pA.x + pB.x, pA.y + pB.y, pA.z + pB.z);
		public static Vector3 Add(params Vector3[] pVectors)
		{
			Vector3 vector = Vector3.zero;
			foreach (Vector3 v in pVectors)
			{
				vector = Add(vector, v);
			}
			return vector;
		}
		public static Vector3 Add(this Vector3 pVector, params Vector3[] pVectors) => Add(pVectors) + pVector;
		public static Vector3 Add(IEnumerable<Vector3> pVectors)
		{
			Vector3 vector = Vector3.zero;
			foreach (Vector3 v in pVectors)
			{
				vector = Add(vector, v);
			}
			return vector;
		}
		public static Vector3 Add(this Vector3 pVector, IEnumerable<Vector3> pVectors) => Add(pVectors) + pVector;

		public static Vector3 Combine(Vector2 pXZ, float pY)
		{
			return new Vector3(pXZ.x, pY, pXZ.y);
		}

		public static Vector3 Scale(this Vector3 pVector, float pScalarX, float pScalarY, float pScalarZ)
		{
			pVector.x *= pScalarX;
			pVector.y *= pScalarY;
			pVector.z *= pScalarZ;
			return pVector;
		}
		public static Vector3 Scale(this Vector3 pVector, float pScalarXZ, float pScalarY)
		{
			pVector.x *= pScalarXZ;
			pVector.y *= pScalarY;
			pVector.z *= pScalarXZ;
			return pVector;
		}

		public static Quaternion Difference(this Quaternion pTo, Quaternion pFrom) => pTo * Quaternion.Inverse(pFrom);
		public static Quaternion Add(this Quaternion pStart, Quaternion pDiff) => pDiff * pStart;
		public static Quaternion UpForwardRotation(Vector3 pForward, Vector3 pUp) => Quaternion.FromToRotation(Vector3.up, pUp) * Quaternion.LookRotation(pForward);
		public static Quaternion Inverse(this Quaternion pRotation) => Quaternion.Inverse(pRotation);

		public static float Add(this float pValue, IEnumerable<float> pDeltas)
		{
			foreach (float v in pDeltas)
			{
				pValue += v;
			}
			return pValue;
		}
		public static float Add(IEnumerable<float> pDeltas)
		 => Add(0.0f, pDeltas);

		public static int Add(this int pValue, IEnumerable<int> pDeltas)
		{
			foreach (int v in pDeltas)
			{
				pValue += v;
			}
			return pValue;
		}
		public static int Add(IEnumerable<int> pDeltas)
		 => Add(0, pDeltas);

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
		public static bool Approximately(this float pA, float pB) => (pA - pB).IsNearZero();
		public static bool ApproximatelyOrGreaterThan(this float pA, float pB) => pA > pB || (pA - pB).IsNearZero();
		public static bool ApproximatelyOrLessThan(this float pA, float pB) => pA < pB || (pA - pB).IsNearZero();
		public static bool Approximately(this int pA, int pB) => (pA - pB).IsNearZero();
		public static bool ApproximatelyOrGreaterThan(this int pA, int pB) => pA > pB || (pA - pB).IsNearZero();
		public static bool ApproximatelyOrLessThan(this int pA, int pB) => pA < pB || (pA - pB).IsNearZero();

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

		public static bool DistanceXZEqual(this Vector3 pA, Vector3 pB, float pDistance) =>
			Horizontal2D(pA - pB).sqrMagnitude == Mathf.Pow(pDistance, 2);
		public static bool DistanceXZGreaterThan(this Vector3 pA, Vector3 pB, float pDistance) =>
			Horizontal2D(pA - pB).sqrMagnitude > Mathf.Pow(pDistance, 2);
		public static bool DistanceXZEqualGreaterThan(this Vector3 pA, Vector3 pB, float pDistance) =>
			Horizontal2D(pA - pB).sqrMagnitude >= Mathf.Pow(pDistance, 2);
		public static bool DistanceXZLessThan(this Vector3 pA, Vector3 pB, float pDistance) =>
			Horizontal2D(pA - pB).sqrMagnitude < Mathf.Pow(pDistance, 2);
		public static bool DistanceXZEqualLessThan(this Vector3 pA, Vector3 pB, float pDistance) =>
			Horizontal2D(pA - pB).sqrMagnitude <= Mathf.Pow(pDistance, 2);

		public static bool IsNearZero(this float pValue) => pValue >= 0.0f ? pValue <= NEARZERO : pValue >= -NEARZERO;
		public static bool IsNearZero(this int pValue) => pValue >= 0 ? pValue <= NEARZERO : pValue >= -NEARZERO;
		public static bool IsNearZero(this Vector2 pVector) => pVector.sqrMagnitude <= NEARZERO;
		public static bool IsNearZero(this Vector3 pVector) => pVector.sqrMagnitude <= NEARZERO;
		public static bool IsNearZeroXZ(this Vector3 pVector)
		{
			pVector.y = 0.0f;
			return pVector.sqrMagnitude <= NEARZERO;
		}
		#endregion Compare
	}
}