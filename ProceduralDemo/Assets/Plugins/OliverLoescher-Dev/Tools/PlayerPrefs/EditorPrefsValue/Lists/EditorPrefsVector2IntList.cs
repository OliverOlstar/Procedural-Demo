using UnityEngine;
using UnityEditor;

namespace ODev
{
	public sealed class EditorPrefsVector2IntList : EditorPrefsList<Vector2Int>
	{
		public EditorPrefsVector2IntList(string key)
			: base(key)
		{
		}

		protected override Vector2Int ReadValue(int index)
		{
			Vector2Int value = Vector2Int.zero;
			value.x = EditorPrefs.GetInt($"{Key}{index}_x", 0);
			value.y = EditorPrefs.GetInt($"{Key}{index}_y", 0);
			return value;
		}

		protected override void WriteValue(int index, Vector2Int value)
		{
			EditorPrefs.SetInt($"{Key}{index}_x", value.x);
			EditorPrefs.SetInt($"{Key}{index}_y", value.y);
		}
	}
}
