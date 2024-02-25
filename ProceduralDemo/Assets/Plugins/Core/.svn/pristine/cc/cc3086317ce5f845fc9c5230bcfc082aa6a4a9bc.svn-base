using UnityEditor;

namespace Core
{
	public sealed class EditorPrefsStringList : EditorPrefsList<string>
	{
		public EditorPrefsStringList(string key)
			: base(key)
		{
		}

		protected override string ReadValue(int index)
		{
			return EditorPrefs.GetString(Key + index, string.Empty);
		}

		protected override void WriteValue(int index, string value)
		{
			EditorPrefs.GetString(Key + index, value);
		}
	}
}
