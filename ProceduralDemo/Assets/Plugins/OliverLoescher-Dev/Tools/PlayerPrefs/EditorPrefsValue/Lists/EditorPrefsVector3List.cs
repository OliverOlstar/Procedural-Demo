using UnityEngine;
using UnityEditor;

namespace ODev
{
	public sealed class EditorPrefsVector3List : EditorPrefsList<Vector3>
	{
		public EditorPrefsVector3List(string key)
			: base(key)
		{
		}

		protected override Vector3 ReadValue(int index)
		{
			Vector3 value = Vector3.zero;
			value.x = EditorPrefs.GetFloat($"{Key}{index}_x", 0.0f);
			value.y = EditorPrefs.GetFloat($"{Key}{index}_y", 0.0f);
			value.z = EditorPrefs.GetFloat($"{Key}{index}_z", 0.0f);
			return value;
		}

		protected override void WriteValue(int index, Vector3 value)
		{
			EditorPrefs.SetFloat($"{Key}{index}_x", value.x);
			EditorPrefs.SetFloat($"{Key}{index}_y", value.y);
			EditorPrefs.SetFloat($"{Key}{index}_z", value.z);
		}
	}
}
