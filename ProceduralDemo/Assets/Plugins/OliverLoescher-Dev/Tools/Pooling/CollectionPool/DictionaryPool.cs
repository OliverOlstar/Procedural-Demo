using System.Collections.Generic;

namespace ODev
{
	public sealed class DictionaryPool<TKey, TValue> : CollectionPool<Dictionary<TKey, TValue>, KeyValuePair<TKey, TValue>>
	{
	}
}
