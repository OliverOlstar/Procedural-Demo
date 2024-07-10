using System;
using UnityEditor;

namespace OCore
{
	public sealed class EditorPrefsEnum<T> : EditorPrefsValue<T> where T : struct, Enum
	{
		public EditorPrefsEnum(string key)
			: this(key, default)
		{
		}

		public EditorPrefsEnum(string key, T defaultValue)
			: base(key, defaultValue)
		{
		}

		protected override T Get()
		{
			string value = EditorPrefs.GetString(Key, DefaultValue.ToString());
			Enum.TryParse(value, out T result);
			return result;
		}

		protected override void Set(T value)
		{
			EditorPrefs.SetString(Key, value.ToString());
		}
	}
}
