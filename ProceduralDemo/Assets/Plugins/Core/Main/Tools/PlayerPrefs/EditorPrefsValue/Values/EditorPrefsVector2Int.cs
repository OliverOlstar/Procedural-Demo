using UnityEngine;
using UnityEditor;

namespace Core
{
	public class EditorPrefsVector2Int : EditorPrefsValue<Vector2Int>
	{
		public EditorPrefsVector2Int(string key)
			: this(key, Vector2Int.zero)
		{
		}

		public EditorPrefsVector2Int(string key, Vector2Int defaultValue)
			: base(key, defaultValue)
		{
		}

		protected override Vector2Int Get()
		{
			Vector2Int value = Vector2Int.zero;
			value.x = EditorPrefs.GetInt($"{Key}_x", DefaultValue.x);
			value.y = EditorPrefs.GetInt($"{Key}_y", DefaultValue.y);
			return value;
		}

		protected override void Set(Vector2Int value)
		{
			EditorPrefs.SetInt($"{Key}_x", value.x);
			EditorPrefs.SetInt($"{Key}_y", value.y);
		}
	}
}