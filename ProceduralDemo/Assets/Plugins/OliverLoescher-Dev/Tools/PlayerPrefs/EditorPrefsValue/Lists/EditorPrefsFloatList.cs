using UnityEditor;

namespace ODev
{
	public sealed class EditorPrefsFloatList : EditorPrefsList<float>
	{
		public EditorPrefsFloatList(string key)
			: base(key)
		{
		}

		protected override float ReadValue(int index)
		{
			return EditorPrefs.GetFloat(Key + index, 0);
		}

		protected override void WriteValue(int index, float value)
		{
			EditorPrefs.SetFloat(Key + index, value);
		}
	}
}
