using UnityEngine;
using UnityEditor;

namespace OCore
{
	public sealed class EditorPrefsColorList : EditorPrefsList<Color>
	{
		public EditorPrefsColorList(string key)
			: base(key)
		{
		}

		protected override Color ReadValue(int index)
		{
			Color value = Color.black;
			value.r = EditorPrefs.GetFloat($"{Key}{index}_r", 0.0f);
			value.g = EditorPrefs.GetFloat($"{Key}{index}_g", 0.0f);
			value.b = EditorPrefs.GetFloat($"{Key}{index}_b", 0.0f);
			value.a = EditorPrefs.GetFloat($"{Key}{index}_a", 0.0f);
			return value;
		}

		protected override void WriteValue(int index, Color value)
		{
			EditorPrefs.SetFloat($"{Key}{index}_r", value.r);
			EditorPrefs.SetFloat($"{Key}{index}_g", value.g);
			EditorPrefs.SetFloat($"{Key}{index}_b", value.b);
			EditorPrefs.SetFloat($"{Key}{index}_a", value.a);
		}
	}
}
