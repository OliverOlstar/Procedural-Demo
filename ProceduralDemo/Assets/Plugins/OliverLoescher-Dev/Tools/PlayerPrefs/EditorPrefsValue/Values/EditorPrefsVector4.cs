using UnityEngine;
using UnityEditor;

namespace ODev
{
	public class EditorPrefsVector4 : EditorPrefsValue<Vector4>
	{
		public EditorPrefsVector4(string key)
			: this(key, Vector4.zero)
		{
		}

		public EditorPrefsVector4(string key, Vector4 defaultValue)
			: base(key, defaultValue)
		{
		}

		protected override Vector4 Get()
		{
			Vector4 value = Vector4.zero;
			value.x = EditorPrefs.GetFloat($"{Key}_x", DefaultValue.x);
			value.y = EditorPrefs.GetFloat($"{Key}_y", DefaultValue.y);
			value.z = EditorPrefs.GetFloat($"{Key}_z", DefaultValue.z);
			value.w = EditorPrefs.GetFloat($"{Key}_w", DefaultValue.w);
			return value;
		}

		protected override void Set(Vector4 value)
		{
			EditorPrefs.SetFloat($"{Key}_x", value.x);
			EditorPrefs.SetFloat($"{Key}_y", value.y);
			EditorPrefs.SetFloat($"{Key}_z", value.z);
			EditorPrefs.SetFloat($"{Key}_w", value.w);
		}
	}
}