using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{

	[Serializable]
	public class AutoDictionaryForEditor
	{
		
	}
	
	[Serializable]
	public abstract class ScriptableObjectDictionaryForEditor
	{
			
	}
		
	[Serializable]
	public abstract class ScriptableObjectDictionary<DictKey, DictValue> : ScriptableObjectDictionaryForEditor where DictKey : ScriptableObject
	{
		public DictKey key;
		public DictValue value;
	}
	
	[Serializable]
	public class ScriptableObjectAutoDictionary<ParentClass, DictKey, DictValue> : AutoDictionaryForEditor, IEnumerable<KeyValuePair<DictKey, DictValue>>
		where DictKey : ScriptableObject 
		where ParentClass : ScriptableObjectDictionary<DictKey, DictValue>, new()
	{
		public List<ParentClass> serializedList;

		private Dictionary<DictKey, DictValue> dict = null;

		private void Init()
		{
			if (dict != null && Application.isPlaying) // At edit time constantly rebuild dict
			{
				return;
			}
			dict = new Dictionary<DictKey, DictValue>();
			for (int i = 0; i < serializedList.Count; ++i)
			{
				DictKey key = serializedList[i].key;
				if (key == null)
				{
					Debug.LogWarning($"ScriptableObjectAutoDictionary.Init() Null key");
					continue;
				}
				if (dict.ContainsKey(key))
				{
					Core.DebugUtil.DevException($"ScriptableObjectAutoDictionary.Init() Duplicate key {key.name}");
					continue;
				}
				dict.Add(key, serializedList[i].value);
			}
		}

		private void Dirty()
		{
			dict = null;
		}

		public DictValue Get(DictKey so)
		{
			Init();
			return dict[so];
		}

		public bool TryGet(DictKey so, out DictValue value)
		{
			Init();
			return dict.TryGetValue(so, out value);
		}

		public void Set(DictKey key, DictValue value)
		{
			if (key == null)
			{
				return;
			}
			Dirty();
			foreach (ParentClass item in serializedList)
			{
				if (item.key != null &&
					item.key.GetInstanceID() == key.GetInstanceID())
				{
					item.value = value;
					return;
				}
			}
			serializedList.Add(new ParentClass() { key = key, value = value });
		}

		public IEnumerator<KeyValuePair<DictKey, DictValue>> GetEnumerator()
		{
			Init();
			return dict.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			Init();
			return dict.GetEnumerator();
		}
	}

}
