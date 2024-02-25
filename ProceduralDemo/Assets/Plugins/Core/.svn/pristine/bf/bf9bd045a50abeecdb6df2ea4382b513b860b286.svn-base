using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Act2
{
	public static class EditorUtil
	{
		private static Texture s_Icon = null;
		public static Texture ActTreeIcon
		{
			get
			{
				if (s_Icon == null)
				{
					s_Icon = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Plugins/Core/ActTree2/Runtime/ActTreeIcon.png");
				}
				return s_Icon;
			}
		}
	}
}
