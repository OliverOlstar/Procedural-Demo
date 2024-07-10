using UnityEditor;

namespace OCore
{
	public sealed class EditorPrefsBoolList : EditorPrefsList<bool>
	{
		public EditorPrefsBoolList(string key)
			: base(key)
		{
		}

		protected override bool ReadValue(int index)
		{
			return EditorPrefs.GetBool(Key + index, false);
		}

		protected override void WriteValue(int index, bool value)
		{
			EditorPrefs.GetBool(Key + index, value);
		}
	}
}
