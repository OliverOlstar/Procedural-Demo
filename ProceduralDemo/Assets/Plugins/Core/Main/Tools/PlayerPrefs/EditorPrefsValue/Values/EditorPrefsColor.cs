using UnityEngine;
using UnityEditor;

namespace Core
{
	public class EditorPrefsColor : EditorPrefsValue<Color>
	{
		public EditorPrefsColor(string key)
			: this(key, Color.white)
		{
		}

		public EditorPrefsColor(string key, Color defaultValue)
			: base(key, defaultValue)
		{
		}

		protected override Color Get()
		{
			Color value = Color.white;
			value.r = EditorPrefs.GetFloat($"{Key}_r", DefaultValue.r);
			value.g = EditorPrefs.GetFloat($"{Key}_g", DefaultValue.g);
			value.b = EditorPrefs.GetFloat($"{Key}_b", DefaultValue.b);
			value.a = EditorPrefs.GetFloat($"{Key}_a", DefaultValue.a);
			return value;
		}

		protected override void Set(Color value)
		{
			EditorPrefs.SetFloat($"{Key}_r", value.r);
			EditorPrefs.SetFloat($"{Key}_g", value.g);
			EditorPrefs.SetFloat($"{Key}_b", value.b);
			EditorPrefs.SetFloat($"{Key}_a", value.a);
		}
	}
}