using UnityEngine;
using UnityEditor;

namespace ODev
{
	public class EditorPrefsVector2 : EditorPrefsValue<Vector2>
	{
		public EditorPrefsVector2(string key)
			: this(key, Vector2.zero)
		{
		}

		public EditorPrefsVector2(string key, Vector2 defaultValue)
			: base(key, defaultValue)
		{
		}

		protected override Vector2 Get()
		{
			Vector2 value = Vector2.zero;
			value.x = EditorPrefs.GetFloat($"{Key}_x", DefaultValue.x);
			value.y = EditorPrefs.GetFloat($"{Key}_y", DefaultValue.y);
			return value;
		}

		protected override void Set(Vector2 value)
		{
			EditorPrefs.SetFloat($"{Key}_x", value.x);
			EditorPrefs.SetFloat($"{Key}_y", value.y);
		}
	}
}