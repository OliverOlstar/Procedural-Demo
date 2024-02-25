using UnityEditor;

namespace Core
{
	public sealed class EditorPrefsInt : EditorPrefsValue<int>
	{
		public EditorPrefsInt(string key)
			: this(key, 0)
		{
		}

		public EditorPrefsInt(string key, int defaultValue)
			: base(key, defaultValue)
		{
		}

		protected override int Get()
		{
			return EditorPrefs.GetInt(Key, DefaultValue);
		}

		protected override void Set(int value)
		{
			EditorPrefs.SetInt(Key, value);
		}
	}
}
