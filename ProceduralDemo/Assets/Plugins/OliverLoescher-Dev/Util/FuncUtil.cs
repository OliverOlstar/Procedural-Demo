using System;
using System.Collections.Generic;
using Sirenix.Utilities;
using UnityEngine;

namespace ODev.Util
{
	public static class Func
	{
		#region Application
		private static bool s_IsApplicationQuitting = false;
		public static bool IsApplicationQuitting => s_IsApplicationQuitting;

		[RuntimeInitializeOnLoadMethod]
		static void RunOnStart()
		{
			s_IsApplicationQuitting = false;
			Application.quitting += Quit;
		}
		private static void Quit()
		{
			s_IsApplicationQuitting = true;
			Application.quitting -= Quit;
		}
		#endregion Application

		public static bool IsRelease()
		{
#if RELEASE
			return true;
#else
			return false;
#endif
		}

		public static float SpringDamper(this float pFrom, float pTo, ref float rVelocity, float pSpring, float pDamper, float pDeltaTime)
		{
			float differance = pFrom - pTo;
			float magnitude = Mathf.Abs(differance);
			if (magnitude > Math.NEARZERO)
			{
				float force = magnitude * pSpring;
				float direction = differance / magnitude;
				rVelocity += (-force * direction) - (rVelocity * pDamper);
			}
			else
			{
				rVelocity -= rVelocity * pDamper;
			}
			pFrom += rVelocity * pDeltaTime;
			return pFrom;
		}
		public static Vector2 SpringDamper(this Vector2 pFrom, Vector2 pTo, ref Vector2 rVelocity, float pSpring, float pDamper, float pDeltaTime)
		{
			Vector2 differance = pFrom - pTo;
			if (differance.sqrMagnitude > Math.NEARZERO)
			{
				float magnitude = differance.magnitude;
				float force = magnitude * pSpring;
				Vector2 direction = differance / magnitude;

				rVelocity += (-force * direction) - (rVelocity * pDamper);
			}
			else
			{
				rVelocity -= rVelocity * pDamper;
			}
			if (rVelocity.sqrMagnitude > Math.NEARZERO)
			{
				pFrom += rVelocity * pDeltaTime;
			}
			return pFrom;
		}
		public static Vector3 SpringDamper(this Vector3 pFrom, Vector3 pTo, ref Vector3 rVelocity, float pSpring, float pDamper, float pDeltaTime)
		{
			Vector3 differance = pFrom - pTo;
			if (differance.sqrMagnitude > Math.NEARZERO)
			{
				float magnitude = differance.magnitude;
				float force = magnitude * pSpring;
				Vector3 direction = differance / magnitude;

				rVelocity += (-force * direction) - (rVelocity * pDamper);
			}
			else
			{
				rVelocity -= rVelocity * pDamper;
			}
			if (rVelocity.sqrMagnitude > Math.NEARZERO)
			{
				pFrom += rVelocity * pDeltaTime;
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

		public static bool AddUniqueItem<T>(this List<T> self, T item)
		{
			if (self.Contains(item))
			{
				return false;
			}
			self.Add(item);
			return true;
		}

		public static bool TryGetAndRemove<TKey, TValue>(ref Dictionary<TKey, TValue> rDictionary, TKey pKey, out TValue pValue)
		{
			if (rDictionary.TryGetValue(pKey, out pValue) && rDictionary.Remove(pKey))
			{
				return true;
			}
			return false;
		}

		public static TValue GetAndRemove<TKey, TValue>(ref Dictionary<TKey, TValue> rDictionary, TKey pKey)
		{
			rDictionary.TryGetValue(pKey, out TValue pValue);
			rDictionary.Remove(pKey);
			return pValue;
		}
		

		public static T GetOrAddComponent<T>(this GameObject pObject) where T : Component
		{
			if (!pObject.TryGetComponent(out T component))
			{
				component = pObject.AddComponent<T>();
			}
			return component;
		}
		public static void GetOrAddComponent<T>(this GameObject pObject, out T pComponent) where T : Component
			=> pComponent = pObject.GetOrAddComponent<T>();

		public static bool TryGetComponentInChildren<T>(this GameObject pObject, out T pComponent) where T : Component
		{
			pComponent = pObject.GetComponentInChildren<T>();
			return pComponent != null;
		}

		public static bool TryGetComponentInParent<T>(this GameObject pObject, out T pComponent) where T : Component
		{
			pComponent = pObject.GetComponentInParent<T>();
			return pComponent != null;
		}

		/// <summary> Checks full collection starting at pStartAtIndex, -1 if failed </summary>
		public static int IndexOf<T>(this T[] pElements, int pStartAtIndex, T pElement = null) where T : class
			=> Foreach(pElements, pStartAtIndex, (T pItem, int _) => pItem == pElement);

		/// <summary> Checks full collection starting at pStartAtIndex, -1 if failed </summary>
		public static int IndexOf<T>(this List<T> pElements, int pStartAtIndex, T pElement = null) where T : class
			=> Foreach(pElements, pStartAtIndex, (T pItem, int _) => pItem == pElement);

		/// <summary> Checks full collection starting at pStartAtIndex, -1 if failed </summary>
		public static int IndexOf<T>(ref T[] rElements, int pStartAtIndex, Func<T, bool> pPredicate)
			=> Foreach(rElements, pStartAtIndex, (T pItem, int _) => !pPredicate(pItem));

		/// <summary> Checks full collection starting at pStartAtIndex, -1 if failed </summary>
		public static int IndexOf<T>(this List<T> pElements, int pStartAtIndex, Func<T, bool> pPredicate)
			=> Foreach(pElements, pStartAtIndex, (T pItem, int _) => !pPredicate(pItem));

		/// <summary> Iterate through collection starting at an index, returning false in predicate ends the loop </summary>
		public static int Foreach<T>(this T[] pElements, int pStartAtIndex, Func<T, int, bool> pPredicate)
		{
			if (pElements.IsNullOrEmpty())
			{
				return -1;
			}
			pStartAtIndex.Loop(pElements.Length);
			for (int i = pStartAtIndex; i < pElements.Length; i++)
			{
				if (!pPredicate.Invoke(pElements[i], i))
				{
					return i;
				}
			}
			for (int i = 0; i < pStartAtIndex; i++)
			{
				if (!pPredicate.Invoke(pElements[i], i))
				{
					return i;
				}
			}
			return -1;
		}

		/// <summary> Iterate through collection starting at an index, returning false in predicate ends the loop </summary>
		public static int Foreach<T>(this List<T> pElements, int pStartAtIndex, Func<T, int, bool> pPredicate)
		{
			if (pElements.IsNullOrEmpty())
			{
				return -1;
			}
			pStartAtIndex.Loop(pElements.Count);
			for (int i = pStartAtIndex; i < pElements.Count; i++)
			{
				if (!pPredicate.Invoke(pElements[i], i))
				{
					return i;
				}
			}
			for (int i = 0; i < pStartAtIndex; i++)
			{
				if (!pPredicate.Invoke(pElements[i], i))
				{
					return i;
				}
			}
			return -1;
		}
	}
}