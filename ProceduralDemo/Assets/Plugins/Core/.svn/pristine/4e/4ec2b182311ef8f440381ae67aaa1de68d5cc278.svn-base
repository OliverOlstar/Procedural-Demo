namespace Data
{
	public abstract class DataBaseBin<TSingleton, TData> : 
	#if UNITY_2021_1_OR_NEWER
		DBDict<TSingleton, DataImporterList2021<TData>, TData>
	#else
		DBDict<TSingleton, DataImporterList<TData>, TData>
	#endif
		where TSingleton : DataBaseBin<TSingleton, TData>
		where TData : class, IDataDictItem
	{
	}
}
