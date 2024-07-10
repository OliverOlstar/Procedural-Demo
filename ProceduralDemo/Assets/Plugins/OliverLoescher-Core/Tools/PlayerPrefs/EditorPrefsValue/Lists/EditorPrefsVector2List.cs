using UnityEngine;
using UnityEditor;

namespace OCore
{
	public sealed class EditorPrefsVector2List : EditorPrefsList<Vector2>
	{
		public EditorPrefsVector2List(string key)
			: base(key)
		{
		}

		protected override Vector2 ReadValue(int index)
		{
			Vector2 value = Vector2.zero;
			value.x = EditorPrefs.GetFloat($"{Key}{index}_x", 0.0f);
			value.y = EditorPrefs.GetFloat($"{Key}{index}_y", 0.0f);
			return value;
		}

		protected override void WriteValue(int index, Vector2 value)
		{
			EditorPrefs.SetFloat($"{Key}{index}_x", value.x);
			EditorPrefs.SetFloat($"{Key}{index}_y", value.y);
		}
	}
}
