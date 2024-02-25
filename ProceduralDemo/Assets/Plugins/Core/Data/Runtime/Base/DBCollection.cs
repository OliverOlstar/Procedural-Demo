
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

namespace Data
{
	public abstract class DBCollection<TSingleton, TImporter, TCollections, TCollection, TData> :
		DBSingleton<TSingleton, TImporter, TCollections, TData>
			where TSingleton : DBCollection<TSingleton, TImporter, TCollections, TCollection, TData>
			where TImporter : IDataImporter<TCollections, TData>, new()
			where TCollections : DataCollections<TCollection, TData>
			where TCollection : DataCollection<TData>
			where TData : IDataDictItem, IDataCollectionItem
	{
		private CollectionDict<TCollections, TCollection, TData> m_Collections = null;

		public IReadOnlyDictionary<string, TCollection> DataCollections => m_Collections;

		protected DBCollection()
		{
			m_Collections = new CollectionDict<TCollections, TCollection, TData>();
		}

		#region static
		public static IEnumerable<TCollection> CollectionValues => Instance.m_Collections.Values;

		public static TCollection GetCollection(string key)
		{
			if (string.IsNullOrEmpty(key)) // Explicitly don't allow empty string keys
			{
				Core.DebugUtil.DevExceptionFormat("{0}.GetCollection() Cannot pass a null or empty Collection ID", typeof(TSingleton).Name);
			}
			if (!Instance.m_Collections.TryGetValue(key, out TCollection rt))
			{
				Core.DebugUtil.DevExceptionFormat("{0}.GetCollection() Data {1} does not exist", typeof(TSingleton).Name, key);
			}
			return rt;
		}

		public static bool TryGetCollection(string key, out TCollection data)
		{
			if (string.IsNullOrEmpty(key)) // Explicitly don't allow empty string keys
			{
				data = null;
				return false;
			}
			return Instance.m_Collections.TryGetValue(key, out data);
		}

		public static bool ContainsCollection(string key)
		{
			if (string.IsNullOrEmpty(key)) // Explicitly don't allow empty string keys
			{
				return false;
			}
			return Instance.m_Collections.ContainsKey(key);
		}

		public static IEnumerable<TCollection> GetCollections() { return Instance.m_Collections.Values; }

		public static IEnumerable<TData> GetValues()
		{
			foreach (TCollection collection in Instance.m_Collections.Values)
			{
				foreach (TData data in collection)
				{
					yield return data;
				}
			}
		}

		public static int GetCollectionCount(string key)
		{
			return TryGetCollection(key, out TCollection collection) ? collection.Count : 0;
		}
		#endregion

		#region DBBase
		sealed public override string SerializeDefaultServerJson()
		{
			List<TData> list = Core.ListPool<TData>.Request();
			foreach (TCollection collection in m_Collections.Values)
			{
				foreach (TData data in collection)
				{
					list.Add(data);
				}
			}
			string json = JsonConvert.SerializeObject(list, Formatting.Indented);
			Core.ListPool<TData>.Return(list);
			return json;
		}

		public override bool EditorTryGetPrimaryKey(string key, out object data)
		{
			data = m_Collections.TryGetValue(key, out TCollection collection) ? collection : null;
			return data != null;
		}
		public override bool EditorTryGetData(string key, out IDataDictItem data)
		{
			foreach (TCollection collection in Instance.m_Collections.Values)
			{
				foreach (TData d in collection)
				{
					if (string.Equals(d.ID, key))
					{
						data = d;
						return true;
					}
				}
			}
			data = null;
			return false;
		}

		public override IEnumerable<string> GetEditorPickerIDs() => m_Collections.Keys;
		#endregion

		internal protected override void InitializeInternal(IEnumerable<TCollections> rawData, int dataCount)
		{
			int count = 0;
			foreach (TCollections collections in rawData)
			{
				count += collections.CollectionCount;
			}
			m_Collections = new CollectionDict<TCollections, TCollection, TData>(rawData, count);
		}

		protected internal override void PostProcessInternal(TCollections rawData)
		{
			foreach (TCollection collection in rawData.Collections)
			{
				if (collection is IDataPostImport post)
				{
					post.OnDataPostImport();
				}
			}
		}
	}

	public class CollectionDict<TCollections, TCollection, TData> : Dictionary<string, TCollection> 
		where TCollections : DataCollections<TCollection, TData>
		where TCollection : DataCollection<TData>
	{
		public CollectionDict() : base() { }
		public CollectionDict(int count) : base(count) { }

		// Grrrrr I hate how this needs to work
		// I don't want to allow Collections to have the ID empty string, 
		// it leads to weird looking data, and unexpecting things happening when empty string is a valid ID
		// However the Oko's server is dependent on this behaviour in some cases... so wrote this class so it blocks
		// the client from looking up collections by empty string, but it's constructor still allows filling the datat structing
		// with empty string keyed data so you can still find this data when enumerating through the structure (which is what we do when exporting to the server)
		public CollectionDict(IEnumerable<TCollections> fill, int collectionCount) : base(collectionCount)
		{
			foreach (TCollections collections in fill)
			{
				foreach (TCollection collection in collections.Collections)
				{
					if (!base.ContainsKey(collection.CollectionID)) // Hacky part use base.ContainsKey() here
					{
						Add(collection.CollectionID, collection);
					}
				}
			}
		}

		new public bool ContainsKey(string key)
		{
			if (string.IsNullOrEmpty(key))
			{
				throw new System.ArgumentNullException($"{GetType().Name}.ContainsKey() Key is null or empty");
			}
			return base.ContainsKey(key);
		}

		new public TCollection this[string key]
		{
			get
			{
				if (string.IsNullOrEmpty(key))
				{
					throw new System.ArgumentNullException($"{GetType().Name}[] Key is null or empty");
				}
				return base[key];
			}
		}

		new public bool TryGetValue(string key, out TCollection value)
		{
			if (string.IsNullOrEmpty(key))
			{
				throw new System.ArgumentNullException($"{GetType().Name}.TryGetValue() Key is null or empty");
			}
			return base.TryGetValue(key, out value);
		}
	}
}
