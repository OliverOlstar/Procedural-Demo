using UnityEngine;
using UnityEditor;

namespace OCore
{
	public class EditorPrefsVector3 : EditorPrefsValue<Vector3>
	{
		public EditorPrefsVector3(string key)
			: this(key, Vector3.zero)
		{
		}

		public EditorPrefsVector3(string key, Vector3 defaultValue)
			: base(key, defaultValue)
		{
		}

		protected override Vector3 Get()
		{
			Vector3 value = Vector3.zero;
			value.x = EditorPrefs.GetFloat($"{Key}_x", DefaultValue.x);
			value.y = EditorPrefs.GetFloat($"{Key}_y", DefaultValue.y);
			value.z = EditorPrefs.GetFloat($"{Key}_z", DefaultValue.z);
			return value;
		}

		protected override void Set(Vector3 value)
		{
			EditorPrefs.SetFloat($"{Key}_x", value.x);
			EditorPrefs.SetFloat($"{Key}_y", value.y);
			EditorPrefs.SetFloat($"{Key}_z", value.z);
		}
	}
}