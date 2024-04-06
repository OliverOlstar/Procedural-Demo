using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModelLocator
{
	public static class ModelLocatorTools
	{
		public static DummyModelLocator Editor = null;
	}

	/// <summary>
	/// Just for attaching an inspector
	/// </summary>
	[ExecuteInEditMode]
	public class DummyModelLocator : MonoBehaviour
	{
		public SOModelLocator Source = null;

#if UNITY_EDITOR
		private void Update()
		{
			if (ModelLocatorTools.Editor != this)
			{
				DestroyImmediate(gameObject);
			}
		}

		private void OnDrawGizmos()
		{
			if (UnityEditor.Selection.Contains(gameObject))
			{
				Gizmos.color = Color.yellow;
				Gizmos.DrawSphere(transform.position, 0.05f);
			}
			else
			{
				Gizmos.DrawSphere(transform.position, 0.05f);
				Gizmos.color = Color.red;
				Gizmos.DrawRay(transform.position, transform.right);
				Gizmos.color = Color.green;
				Gizmos.DrawRay(transform.position, transform.up);
				Gizmos.color = Color.blue;
				Gizmos.DrawRay(transform.position, transform.forward);
			}
		}
#endif
	}
}
