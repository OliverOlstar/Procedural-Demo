using UnityEditor;

namespace Core
{
	public sealed class EditorPrefsBool : EditorPrefsValue<bool>
	{
		public EditorPrefsBool(string key)
			: this(key, false)
		{
		}

		public EditorPrefsBool(string key, bool defaultValue)
			: base(key, defaultValue)
		{
		}

		protected override bool Get()
		{
			return EditorPrefs.GetBool(Key, DefaultValue);
		}

		protected override void Set(bool value)
		{
			EditorPrefs.SetBool(Key, value);
		}
	}
}
