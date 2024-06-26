using UnityEngine;

namespace Core
{
	public static class ColorConst
	{
		public static Color FromHex(string hex)
		{
			if (!Str.StartsWith(hex, '#'))
			{
				Debug.LogError("ColorConst.FromHex: Colour hex code doesnt start with a \'#\'.");
			}
			Color color = Color.white;
			ColorUtility.TryParseHtmlString(hex, out color);
			return color;
		}

		public static readonly Color DarkRed = new(0.4f, 0.0f, 0.0f);
		public static readonly Color DarkGreen = new(0.0f, 0.4f, 0.0f);
		public static readonly Color DarkBlue = new(0.0f, 0.0f, 0.4f);
		public static readonly Color Purple = new(0.4f, 0.0f, 0.6f);
		public static readonly Color Orange = FromHex("#FF7800FF");
		public static readonly Color Brown = new(0.5377358f, 0.191421f, 0);
		public static readonly Color Merlot = new Color32(127, 0, 44, 255);
		public static readonly Color BlueGreen = new Color32(0, 80, 80, 255);
		public static readonly Color SeaFoam = new Color32(120, 220, 160, 255);
		public static readonly Color Pink = new Color32(255, 174, 201, 255);
	}
}
