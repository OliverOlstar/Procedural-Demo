using System;
using UnityEditor;

namespace ODev
{
	public sealed class EditorPrefsEnumList<T> : EditorPrefsList<T> where T : struct, Enum
	{
		public EditorPrefsEnumList(string key)
			: base(key)
		{
		}

		protected override T ReadValue(int index)
		{
			string value = EditorPrefs.GetString(Key + index, null);
			Enum.TryParse(value, out T result);
			return result;
		}

		protected override void WriteValue(int index, T value)
		{
			EditorPrefs.GetString(Key + index, value.ToString());
		}
	}
}
