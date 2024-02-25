using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Act2
{
	public class CopySerializedReferenceUtil : ScriptableObject
	{
		[SerializeReference]
		private object m_Reference;

		// This seems like a somewhat hacky way to do this... but has been working well
		public static object CopyObject(object item)
		{
			CopySerializedReferenceUtil instance = ScriptableObject.CreateInstance<CopySerializedReferenceUtil>();
			instance.m_Reference = item;
			CopySerializedReferenceUtil copy = Instantiate(instance);
			object copiedItem = copy.m_Reference;
			DestroyImmediate(instance);
			DestroyImmediate(copy);
			return copiedItem;
		}

		public static T CopyGeneric<T>(T item) where T : class
		{
			return CopyObject(item) as T;
		}
	}
}
