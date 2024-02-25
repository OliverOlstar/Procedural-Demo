using UnityEngine;
using UnityEditor;

namespace Core
{
	public sealed class EditorPrefsVector4List : EditorPrefsList<Vector4>
	{
		public EditorPrefsVector4List(string key)
			: base(key)
		{
		}

		protected override Vector4 ReadValue(int index)
		{
			Vector4 value = Vector4.zero;
			value.x = EditorPrefs.GetFloat($"{Key}{index}_x", 0.0f);
			value.y = EditorPrefs.GetFloat($"{Key}{index}_y", 0.0f);
			value.z = EditorPrefs.GetFloat($"{Key}{index}_z", 0.0f);
			value.w = EditorPrefs.GetFloat($"{Key}{index}_w", 0.0f);
			return value;
		}

		protected override void WriteValue(int index, Vector4 value)
		{
			EditorPrefs.SetFloat($"{Key}{index}_x", value.x);
			EditorPrefs.SetFloat($"{Key}{index}_y", value.y);
			EditorPrefs.SetFloat($"{Key}{index}_z", value.z);
			EditorPrefs.SetFloat($"{Key}{index}_w", value.w);
		}
	}
}
