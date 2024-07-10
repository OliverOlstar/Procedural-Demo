using UnityEngine;
using UnityEditor;

namespace OCore
{
	public sealed class EditorPrefsVector3IntList : EditorPrefsList<Vector3Int>
	{
		public EditorPrefsVector3IntList(string key)
			: base(key)
		{
		}

		protected override Vector3Int ReadValue(int index)
		{
			Vector3Int value = Vector3Int.zero;
			value.x = EditorPrefs.GetInt($"{Key}{index}_x", 0);
			value.y = EditorPrefs.GetInt($"{Key}{index}_y", 0);
			value.z = EditorPrefs.GetInt($"{Key}{index}_z", 0);
			return value;
		}

		protected override void WriteValue(int index, Vector3Int value)
		{
			EditorPrefs.SetInt($"{Key}{index}_x", value.x);
			EditorPrefs.SetInt($"{Key}{index}_y", value.y);
			EditorPrefs.SetInt($"{Key}{index}_z", value.z);
		}
	}
}
