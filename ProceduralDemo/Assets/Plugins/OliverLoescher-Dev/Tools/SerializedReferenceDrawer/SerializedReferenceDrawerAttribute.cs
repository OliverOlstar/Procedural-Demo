#if UNITY_2021_1_OR_NEWER
using UnityEngine;

namespace ODev
{
	public enum SerializedRefGUIStyle
	{
		Foldout = 0,
		Header,
		Flat,
		FlatIndented,
	}

	public class SerializedReferenceDrawerAttribute : PropertyAttribute
	{
		public SerializedRefGUIStyle Style;
		public string NullEntryName;

		public SerializedReferenceDrawerAttribute(SerializedRefGUIStyle style = SerializedRefGUIStyle.Foldout, string nullEntryName = null)
		{
			Style = style;
			NullEntryName = nullEntryName;
		}
	}
}
#endif