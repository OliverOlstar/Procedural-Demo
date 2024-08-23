using UnityEditor;

namespace ODev
{
	public sealed class EditorPrefsFloat : EditorPrefsValue<float>
	{
		public EditorPrefsFloat(string key)
			: this(key, 0f)
		{
		}

		public EditorPrefsFloat(string key, float defaultValue)
			: base(key, defaultValue)
		{
		}

		protected override float Get()
		{
			return EditorPrefs.GetFloat(Key, DefaultValue);
		}

		protected override void Set(float value)
		{
			EditorPrefs.SetFloat(Key, value);
		}
	}
}
