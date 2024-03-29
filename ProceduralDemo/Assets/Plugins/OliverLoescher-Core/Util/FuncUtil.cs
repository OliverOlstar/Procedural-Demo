using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OliverLoescher.Util
{
	public static class Func
	{
		#region Application
		public static bool IsApplicationQuitting = false;
		static void Quit() => IsApplicationQuitting = true;

		[RuntimeInitializeOnLoadMethod]
		static void RunOnStart() { IsApplicationQuitting = false; Application.quitting += Quit; }
		#endregion Application

		public static float SpringDamper(this float pFrom, float pTo, ref float pVelocity, float pSpring, float pDamper, float pDeltaTime)
		{
			float differance = pFrom - pTo;
			float magnitude = Mathf.Abs(differance);
			if (magnitude > Math.NEARZERO)
			{
				float force = magnitude * pSpring;
				float direction = differance / magnitude;

				pVelocity += (-force * direction) - (pVelocity * pDamper);
			}
			else
			{
				pVelocity -= pVelocity * pDamper;
			}
			pFrom += pVelocity * pDeltaTime;
			return pFrom;
		}
		public static Vector2 SpringDamper(this Vector2 pFrom, Vector2 pTo, ref Vector2 pVelocity, float pSpring, float pDamper, float pDeltaTime)
		{
			Vector2 differance = pFrom - pTo;
			if (differance.sqrMagnitude > Math.NEARZERO)
			{
				float magnitude = differance.magnitude;
				float force = magnitude * pSpring;
				Vector2 direction = differance / magnitude;

				pVelocity += (-force * direction) - (pVelocity * pDamper);
			}
			else
			{
				pVelocity -= pVelocity * pDamper;
			}
			if (pVelocity.sqrMagnitude > Math.NEARZERO)
			{
				pFrom += pVelocity * pDeltaTime;
			}
			return pFrom;
		}
		public static Vector3 SpringDamper(this Vector3 pFrom, Vector3 pTo, ref Vector3 pVelocity, float pSpring, float pDamper, float pDeltaTime)
		{
			Vector3 differance = pFrom - pTo;
			if (differance.sqrMagnitude > Math.NEARZERO)
			{
				float magnitude = differance.magnitude;
				float force = magnitude * pSpring;
				Vector3 direction = differance / magnitude;

				pVelocity += (-force * direction) - (pVelocity * pDamper);
			}
			else
			{
				pVelocity -= pVelocity * pDamper;
			}
			if (pVelocity.sqrMagnitude > Math.NEARZERO)
			{
				pFrom += pVelocity * pDeltaTime;
			}
			return pFrom;
		}

		public static float SmoothStep(float pMin, float pMax, float pIn)
		{
			return Mathf.Clamp01((pIn - pMin) / (pMax - pMin));
		}
		public static float SmoothStep(Vector2 pMinMax, float pIn) => SmoothStep(pMinMax.x, pMinMax.y, pIn);

		public static float SafeAngle(float pAngle)
		{
			if (pAngle > 180)
			{
				pAngle -= 360;
			}
			return pAngle;
		}

		public static bool TryGetAndRemove<TKey, TValue>(ref Dictionary<TKey, TValue> pDictionary, TKey pKey, out TValue pValue)
		{
			if (pDictionary.TryGetValue(pKey, out pValue) && pDictionary.Remove(pKey))
			{
				return true;
			}
			return false;
		}

		public static TValue GetAndRemove<TKey, TValue>(ref Dictionary<TKey, TValue> pDictionary, TKey pKey)
		{
			pDictionary.TryGetValue(pKey, out TValue pValue);
			pDictionary.Remove(pKey);
			return pValue;
		}

		/// <summary>
		/// Checks full array starting at pStartAtIndex, -1 if failed
		/// </summary>
		public static int IndexOf<T>(T[] pElements, int pStartAtIndex, T pElement = null) where T : class
		{
			for (int i = pStartAtIndex; i < pElements.Length; i++)
			{
				if (pElements[i] == pElement)
				{
					return i;
				}
			}
			for (int i = 0; i < pStartAtIndex; i++)
			{
				if (pElements[i] == pElement)
				{
					return i;
				}
			}
			return -1;
		}

		/// <summary>
		/// Checks full list starting at pStartAtIndex, -1 if failed
		/// </summary>
		public static int IndexOf<T>(List<T> pElements, int pStartAtIndex, T pElement = null) where T : class
		{
			for (int i = pStartAtIndex; i < pElements.Count; i++)
			{
				if (pElements[i] == pElement)
				{
					return i;
				}
			}
			for (int i = 0; i < pStartAtIndex; i++)
			{
				if (pElements[i] == pElement)
				{
					return i;
				}
			}
			return -1;
		}



		/// <summary>
		/// Checks full array starting at pStartAtIndex, -1 if failed
		/// </summary>
		public static int IndexOf<T>(T[] pElements, int pStartAtIndex, Func<T, bool> pPredicate)
		{
			for (int i = pStartAtIndex; i < pElements.Length; i++)
			{
				if (pPredicate.Invoke(pElements[i]))
				{
					return i;
				}
			}
			for (int i = 0; i < pStartAtIndex; i++)
			{
				if (pPredicate.Invoke(pElements[i]))
				{
					return i;
				}
			}
			return -1;
		}

		/// <summary>
		/// Checks full list starting at pStartAtIndex, -1 if failed
		/// </summary>
		public static int IndexOf<T>(List<T> pElements, int pStartAtIndex, Func<T, bool> pPredicate)
		{
			pStartAtIndex = Mathf.Clamp(pStartAtIndex, 0, pElements.Count);
			for (int i = pStartAtIndex; i < pElements.Count; i++)
			{
				if (pPredicate.Invoke(pElements[i]))
				{
					return i;
				}
			}
			for (int i = 0; i < pStartAtIndex; i++)
			{
				if (pPredicate.Invoke(pElements[i]))
				{
					return i;
				}
			}
			return -1;
		}
	}
}