using UnityEngine;
using UnityEditor;

namespace Core
{
	public class EditorPrefsVector3Int : EditorPrefsValue<Vector3Int>
	{
		public EditorPrefsVector3Int(string key)
			: this(key, Vector3Int.zero)
		{
		}

		public EditorPrefsVector3Int(string key, Vector3Int defaultValue)
			: base(key, defaultValue)
		{
		}

		protected override Vector3Int Get()
		{
			Vector3Int value = Vector3Int.zero;
			value.x = EditorPrefs.GetInt($"{Key}_x", DefaultValue.x);
			value.y = EditorPrefs.GetInt($"{Key}_y", DefaultValue.y);
			value.z = EditorPrefs.GetInt($"{Key}_z", DefaultValue.z);
			return value;
		}

		protected override void Set(Vector3Int value)
		{
			EditorPrefs.SetInt($"{Key}_x", value.x);
			EditorPrefs.SetInt($"{Key}_y", value.y);
			EditorPrefs.SetInt($"{Key}_z", value.z);
		}
	}
}