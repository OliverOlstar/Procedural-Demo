using UnityEditor;

namespace ODev
{
	public sealed class EditorPrefsIntList : EditorPrefsList<int>
	{
		public EditorPrefsIntList(string key)
			: base(key)
		{
		}

		protected override int ReadValue(int index)
		{
			return EditorPrefs.GetInt(Key + index, 0);
		}

		protected override void WriteValue(int index, int value)
		{
			EditorPrefs.SetInt(Key + index, value);
		}
	}
}
