
namespace Core
{
	// It appears that classes which have struct enumerators when cast to IEnumerable<T> cause garbage when enumerating
	// I believe this is probably due to the struct enumerator getting cast to IEnumerator under the hood and boxed?
	// This class is work around so classes can keep their collections private but still expose them for enumeration without generating garbage
	// pulic class Example
	// {
	//		private List<string> m_List;
	//		public IEnumerable<string> CausesGarbage => m_List;
	//		public StructEnumerable.List<string> NoGarbage => m_List;
	// }
	namespace StructEnumerable
	{
		public readonly struct List<T>
		{
			private readonly System.Collections.Generic.List<T> m_Enumerable;

			public List(System.Collections.Generic.List<T> enumerable)
			{
				m_Enumerable = enumerable;
			}

			public System.Collections.Generic.List<T>.Enumerator GetEnumerator()
			{
				return m_Enumerable.GetEnumerator();
			}

			public static implicit operator List<T>(System.Collections.Generic.List<T> enumerable) => new(enumerable);
		}

		public readonly struct Queue<T>
		{
			private readonly System.Collections.Generic.Queue<T> m_Enumerable;

			public Queue(System.Collections.Generic.Queue<T> enumerable)
			{
				m_Enumerable = enumerable;
			}

			public System.Collections.Generic.Queue<T>.Enumerator GetEnumerator()
			{
				return m_Enumerable.GetEnumerator();
			}

			public static implicit operator Queue<T>(System.Collections.Generic.Queue<T> enumerable) => new(enumerable);
		}

		public readonly struct HashSet<T>
		{
			private readonly System.Collections.Generic.HashSet<T> m_Enumerable;

			public HashSet(System.Collections.Generic.HashSet<T> enumerable)
			{
				m_Enumerable = enumerable;
			}

			public System.Collections.Generic.HashSet<T>.Enumerator GetEnumerator()
			{
				return m_Enumerable.GetEnumerator();
			}

			public static implicit operator HashSet<T>(System.Collections.Generic.HashSet<T> enumerable) => new(enumerable);
		}

		public readonly struct Dictionary<K, V>
		{
			private readonly System.Collections.Generic.Dictionary<K, V> m_Enumerable;

			public Dictionary(System.Collections.Generic.Dictionary<K, V> enumerable)
			{
				m_Enumerable = enumerable;
			}

			public System.Collections.Generic.Dictionary<K, V>.Enumerator GetEnumerator()
			{
				return m_Enumerable.GetEnumerator();
			}

			public static implicit operator Dictionary<K, V>(System.Collections.Generic.Dictionary<K, V> enumerable) => new(enumerable);
		}

		public readonly struct DictionaryKeys<K, V>
		{
			private readonly System.Collections.Generic.Dictionary<K, V>.KeyCollection m_Enumerable;

			public DictionaryKeys(System.Collections.Generic.Dictionary<K, V>.KeyCollection enumerable)
			{
				m_Enumerable = enumerable;
			}

			public System.Collections.Generic.Dictionary<K, V>.KeyCollection.Enumerator GetEnumerator()
			{
				return m_Enumerable.GetEnumerator();
			}

			public static implicit operator DictionaryKeys<K, V>(System.Collections.Generic.Dictionary<K, V>.KeyCollection enumerable) => 
				new(enumerable);
		}

		public readonly struct DictionaryValues<K, V>
		{
			private readonly System.Collections.Generic.Dictionary<K, V>.ValueCollection m_Enumerable;

			public DictionaryValues(System.Collections.Generic.Dictionary<K, V>.ValueCollection enumerable)
			{
				m_Enumerable = enumerable;
			}

			public System.Collections.Generic.Dictionary<K, V>.ValueCollection.Enumerator GetEnumerator()
			{
				return m_Enumerable.GetEnumerator();
			}

			public static implicit operator DictionaryValues<K, V>(System.Collections.Generic.Dictionary<K, V>.ValueCollection enumerable) => 
				new(enumerable);
		}
	}
}
