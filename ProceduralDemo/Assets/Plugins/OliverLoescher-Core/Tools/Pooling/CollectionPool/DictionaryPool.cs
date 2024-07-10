using System.Collections.Generic;

namespace OCore
{
	public sealed class DictionaryPool<TKey, TValue> : CollectionPool<Dictionary<TKey, TValue>, KeyValuePair<TKey, TValue>>
	{
	}
}
