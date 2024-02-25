using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Data
{
	public abstract class DataBaseWithCollectionBin<TSingleton, TCollections, TCollection, TData> :
		DBCollection<TSingleton, DataImporterCollection<TCollections, TCollection, TData>, TCollections, TCollection, TData>
			where TSingleton : DataBaseWithCollectionBin<TSingleton, TCollections, TCollection, TData>
			where TCollections : DataCollections<TCollection, TData>
			where TCollection : DataCollection<TData>
			where TData : class, IDataDictItem, IDataCollectionItem
	{
		public static void GetCollectionDataNeighbors(TData sourceData, bool allowInfiniteLoop, out TData previous, out TData next)
		{
			previous = null;
			next = null;
			foreach (TCollection collection in GetCollections())
			{
				if (collection.CollectionID != sourceData.CollectionID)
				{
					//skip different collections
					continue;
				}
				for (int i = 0; i < collection.Count; i++)
				{
					if (collection[i].ID != sourceData.ID)
					{
						//skip different tiers
						continue;
					}

					if (i > 0)
					{
						previous = collection[i - 1];
					}
					else if(allowInfiniteLoop)
					{
						previous = collection[collection.Count - 1];
					}
					else
					{
						previous = null;
					}
					
					if(i < collection.Count - 1)
					{
						next = collection[i + 1];
					}
					else if(allowInfiniteLoop)
					{
						next = collection[0];
					}
					else
					{
						next = null;
					}
					// previous = i > 0 ? collection[i - 1] : collection[collection.Count - 1];
					// next = i < collection.Count - 1 ? collection[i + 1] : collection[0];
					break;
				}
			}
		}
	}

	public abstract class DataBaseWithCollectionAndDictionaryBin<TSingleton, TCollections, TCollection, TData> :
		DBCollectionAndDict<TSingleton, DataImporterCollection<TCollections, TCollection, TData>, TCollections, TCollection, TData>
			where TSingleton : DataBaseWithCollectionAndDictionaryBin<TSingleton, TCollections, TCollection, TData>
			where TCollections : DataCollections<TCollection, TData>
			where TCollection : DataCollection<TData>
			where TData : class, IDataCollectionItem, IDataDictItem
	{
	}

	public abstract class DataBaseWithCollectionAndDictionaryAndListBin<TSingleton, TCollections, TCollection, TData> :
		DBCollectionAndDictAndList<TSingleton, DataImporterCollection<TCollections, TCollection, TData>, TCollections, TCollection, TData>
			where TSingleton : DataBaseWithCollectionAndDictionaryAndListBin<TSingleton, TCollections, TCollection, TData>
			where TCollections : DataCollections<TCollection, TData>
			where TCollection : DataCollection<TData>
			where TData : class, IDataCollectionItem, IDataDictItem
	{
	}
}
