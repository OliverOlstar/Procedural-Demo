using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace ODev.Util
{
	/// <summary>
	/// Public access point for any class to access MonoBehaviour functions
	/// </summary>
	public class Mono : MonoBehaviourSingletonAuto<Mono>
	{
		protected override void OnDestroy()
		{
			StopAllCoroutines();
			base.OnDestroy();
		}

		#region Coroutines
		public static Coroutine Start(in IEnumerator pEnumerator)
		{
			return Instance.StartCoroutine(pEnumerator);
		}
		public static void Stop(ref Coroutine pCoroutine)
		{
			if (pCoroutine == null)
			{
				return;
			}
			Instance.StopCoroutine(pCoroutine);
			pCoroutine = null;
		}
		#endregion Coroutines
	}
}
