using UnityEditor;

namespace Core
{
	public sealed class EditorPrefsString : EditorPrefsValue<string>
	{
		public EditorPrefsString(string key)
			: this(key, string.Empty)
		{
		}

		public EditorPrefsString(string key, string defaultValue)
			: base(key, defaultValue)
		{
		}

		protected override string Get()
		{
			return EditorPrefs.GetString(Key, DefaultValue);
		}

		protected override void Set(string value)
		{
			EditorPrefs.SetString(Key, value);
		}
	}
}
